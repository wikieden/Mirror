﻿// batching functionality encapsulated into one class.
// -> less complexity
// -> easy to test
//
// IMPORTANT: we use THRESHOLD batching, not MAXED SIZE batching.
// see threshold comments below.
//
// includes timestamp for tick batching.
// -> allows NetworkTransform etc. to use timestamp without including it in
//    every single message
using System;
using System.Collections.Generic;

namespace Mirror
{
    public class Batcher
    {
        // batching threshold instead of max size.
        // -> small messages are fit into threshold sized batches
        // -> messages larger than threshold are single batches
        //
        // in other words, we fit up to 'threshold' but still allow larger ones
        // for two reasons:
        // 1.) data races: skipping batching for larger messages would send a
        //     large spawn message immediately, while others are batched and
        //     only flushed at the end of the frame
        // 2) timestamp batching: if each batch is expected to contain a
        //    timestamp, then large messages have to be a batch too. otherwise
        //    they would not contain a timestamp
        readonly int threshold;

        // TimeStamp header size for those who need it
        public const int HeaderSize = sizeof(double);

        // full batches ready to be sent.
        // DO NOT queue NetworkMessage, it would box.
        // DO NOT queue each serialization separately.
        //        it would allocate too many writers.
        //        https://github.com/vis2k/Mirror/pull/3127
        // => best to build batches on the fly.
        Queue<NetworkWriterPooled> batches = new Queue<NetworkWriterPooled>();

        // current batch in progress
        NetworkWriterPooled batch;

        public Batcher(int threshold)
        {
            this.threshold = threshold;
        }

        // add a message for batching
        // we allow any sized messages.
        // caller needs to make sure they are within max packet size.
        public void AddMessage(ArraySegment<byte> message, double timeStamp)
        {
            // put into a (pooled) writer
            // -> WriteBytes instead of WriteSegment because the latter
            //    would add a size header. we want to write directly.
            // -> will be returned to pool when making the batch!
            // IMPORTANT: NOT adding a size header / msg saves LOTS of bandwidth
            //NetworkWriterPooled writer = NetworkWriterPool.Get();
            //writer.WriteBytes(message.Array, message.Offset, message.Count);
            //messages.Enqueue(writer);

            // initialize a new batch if necessary
            if (batch == null)
            {
                // borrow from pool. we return it in GetBatch.
                batch = NetworkWriterPool.Get();

                // write timestamp first.
                // -> double precision for accuracy over long periods of time
                batch.WriteDouble(timeStamp);
            }

            // add serialization to current batc. even if > threshold.
            // (we do allow > threshold sized messages as single batch)
            batch.WriteBytes(message.Array, message.Offset, message.Count);

            // finalize this batch if >= threshold
            if (batch.Position >= threshold)
            {
                batches.Enqueue(batch);
                batch = null;
            }
        }

        // helper function to copy a batch to writer and return it to pool
        static void CopyAndReturn(NetworkWriterPooled batch, NetworkWriter writer)
        {
            // make sure the writer is fresh to avoid uncertain situations
            if (writer.Position != 0)
                throw new ArgumentException($"GetNextBatch needs a fresh writer!");

            // copy to the target writer
            ArraySegment<byte> segment = batch.ToArraySegment();
            writer.WriteBytes(segment.Array, segment.Offset, segment.Count);

            // return batch to pool for reuse
            NetworkWriterPool.Return(batch);
        }

        // get the next batch which is available for sending (if any).
        // TODO safely get & return a batch instead of copying to writer?
        // TODO could return pooled writer & use GetBatch in a 'using' statement!
        public bool GetBatch(NetworkWriter writer)
        {
            // get first batch from queue (if any)
            if (batches.TryDequeue(out NetworkWriterPooled first))
            {
                CopyAndReturn(first, writer);
                return true;
            }

            // if queue was empty, we can send the batch in progress.
            if (batch != null)
            {
                CopyAndReturn(batch, writer);
                batch = null;
                return true;
            }

            // nothing was written
            return false;
        }
    }
}
