using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Mirror
{
    // Mirror's Weaver automatically detects all NetworkReader function types,
    // but they do all need to be extensions.
    public static class NetworkReaderExtensions
    {
        // cache encoding instead of creating it each time
        // 1000 readers before:  1MB GC, 30ms
        // 1000 readers after: 0.8MB GC, 18ms
        static readonly UTF8Encoding encoding = new UTF8Encoding(false, true);

        public static byte ReadByte(this NetworkReader reader) => reader.ReadBlittable<byte>();
        public static byte? ReadByteNullable(this NetworkReader reader) => reader.ReadBlittableNullable<byte>();

        public static sbyte ReadSByte(this NetworkReader reader) => reader.ReadBlittable<sbyte>();
        public static sbyte? ReadSByteNullable(this NetworkReader reader) => reader.ReadBlittableNullable<sbyte>();

        // bool is not blittable. read as ushort.
        public static char ReadChar(this NetworkReader reader) => (char)reader.ReadBlittable<ushort>();
        public static char? ReadCharNullable(this NetworkReader reader) => (char?)reader.ReadBlittableNullable<ushort>();

        // bool is not blittable. read as byte.
        public static bool ReadBool(this NetworkReader reader) => reader.ReadBlittable<byte>() != 0;
        public static bool? ReadBoolNullable(this NetworkReader reader)
        {
            byte? value = reader.ReadBlittableNullable<byte>();
            return value.HasValue ? (value.Value != 0) : default(bool?);
        }

        public static short ReadShort(this NetworkReader reader) => (short)reader.ReadUShort();
        public static short? ReadShortNullable(this NetworkReader reader) => reader.ReadBlittableNullable<short>();

        public static ushort ReadUShort(this NetworkReader reader) => reader.ReadBlittable<ushort>();
        public static ushort? ReadUShortNullable(this NetworkReader reader) => reader.ReadBlittableNullable<ushort>();

        public static int ReadInt(this NetworkReader reader) => reader.ReadBlittable<int>();
        public static int? ReadIntNullable(this NetworkReader reader) => reader.ReadBlittableNullable<int>();

        public static uint ReadUInt(this NetworkReader reader) => reader.ReadBlittable<uint>();
        public static uint? ReadUIntNullable(this NetworkReader reader) => reader.ReadBlittableNullable<uint>();

        public static long ReadLong(this NetworkReader reader) => reader.ReadBlittable<long>();
        public static long? ReadLongNullable(this NetworkReader reader) => reader.ReadBlittableNullable<long>();

        public static ulong ReadULong(this NetworkReader reader) => reader.ReadBlittable<ulong>();
        public static ulong? ReadULongNullable(this NetworkReader reader) => reader.ReadBlittableNullable<ulong>();

        public static float ReadFloat(this NetworkReader reader) => reader.ReadBlittable<float>();
        public static float? ReadFloatNullable(this NetworkReader reader) => reader.ReadBlittableNullable<float>();

        public static double ReadDouble(this NetworkReader reader) => reader.ReadBlittable<double>();
        public static double? ReadDoubleNullable(this NetworkReader reader) => reader.ReadBlittableNullable<double>();

        public static decimal ReadDecimal(this NetworkReader reader) => reader.ReadBlittable<decimal>();
        public static decimal? ReadDecimalNullable(this NetworkReader reader) => reader.ReadBlittableNullable<decimal>();

        /// <exception cref="T:System.ArgumentException">if an invalid utf8 string is sent</exception>
        public static string ReadString(this NetworkReader reader)
        {
            // read number of bytes
            ushort size = reader.ReadUShort();

            // null support, see NetworkWriter
            if (size == 0)
                return null;

            int realSize = size - 1;

            // make sure it's within limits to avoid allocation attacks etc.
            if (realSize >= NetworkWriter.MaxStringLength)
            {
                throw new EndOfStreamException($"ReadString too long: {realSize}. Limit is: {NetworkWriter.MaxStringLength}");
            }

            ArraySegment<byte> data = reader.ReadBytesSegment(realSize);

            // convert directly from buffer to string via encoding
            return encoding.GetString(data.Array, data.Offset, data.Count);
        }

        /// <exception cref="T:OverflowException">if count is invalid</exception>
        public static byte[] ReadBytesAndSize(this NetworkReader reader)
        {
            // count = 0 means the array was null
            // otherwise count -1 is the length of the array
            uint count = reader.ReadUInt();
            // Use checked() to force it to throw OverflowException if data is invalid
            return count == 0 ? null : reader.ReadBytes(checked((int)(count - 1u)));
        }

        public static byte[] ReadBytes(this NetworkReader reader, int count)
        {
            byte[] bytes = new byte[count];
            reader.ReadBytes(bytes, count);
            return bytes;
        }

        /// <exception cref="T:OverflowException">if count is invalid</exception>
        public static ArraySegment<byte> ReadBytesAndSizeSegment(this NetworkReader reader)
        {
            // count = 0 means the array was null
            // otherwise count - 1 is the length of the array
            uint count = reader.ReadUInt();
            // Use checked() to force it to throw OverflowException if data is invalid
            return count == 0 ? default : reader.ReadBytesSegment(checked((int)(count - 1u)));
        }

        public static Vector2 ReadVector2(this NetworkReader reader) => reader.ReadBlittable<Vector2>();
        public static Vector2? ReadVector2Nullable(this NetworkReader reader) => reader.ReadBlittableNullable<Vector2>();

        public static Vector3 ReadVector3(this NetworkReader reader) => reader.ReadBlittable<Vector3>();
        public static Vector3? ReadVector3Nullable(this NetworkReader reader) => reader.ReadBlittableNullable<Vector3>();

        public static Vector4 ReadVector4(this NetworkReader reader) => reader.ReadBlittable<Vector4>();
        public static Vector4? ReadVector4Nullable(this NetworkReader reader) => reader.ReadBlittableNullable<Vector4>();

        public static Vector2Int ReadVector2Int(this NetworkReader reader) => reader.ReadBlittable<Vector2Int>();
        public static Vector2Int? ReadVector2IntNullable(this NetworkReader reader) => reader.ReadBlittableNullable<Vector2Int>();

        public static Vector3Int ReadVector3Int(this NetworkReader reader) => reader.ReadBlittable<Vector3Int>();
        public static Vector3Int? ReadVector3IntNullable(this NetworkReader reader) => reader.ReadBlittableNullable<Vector3Int>();

        public static Color ReadColor(this NetworkReader reader) => reader.ReadBlittable<Color>();
        public static Color? ReadColorNullable(this NetworkReader reader) => reader.ReadBlittableNullable<Color>();

        public static Color32 ReadColor32(this NetworkReader reader) => reader.ReadBlittable<Color32>();
        public static Color32? ReadColor32Nullable(this NetworkReader reader) => reader.ReadBlittableNullable<Color32>();

        public static Quaternion ReadQuaternion(this NetworkReader reader) => reader.ReadBlittable<Quaternion>();
        public static Quaternion? ReadQuaternionNullable(this NetworkReader reader) => reader.ReadBlittableNullable<Quaternion>();

        public static Rect ReadRect(this NetworkReader reader) => reader.ReadBlittable<Rect>();
        public static Rect? ReadRectNullable(this NetworkReader reader) => reader.ReadBlittableNullable<Rect>();

        public static Plane ReadPlane(this NetworkReader reader) => reader.ReadBlittable<Plane>();
        public static Plane? ReadPlaneNullable(this NetworkReader reader) => reader.ReadBlittableNullable<Plane>();

        public static Ray ReadRay(this NetworkReader reader) => reader.ReadBlittable<Ray>();
        public static Ray? ReadRayNullable(this NetworkReader reader) => reader.ReadBlittableNullable<Ray>();

        public static Matrix4x4 ReadMatrix4x4(this NetworkReader reader)=> reader.ReadBlittable<Matrix4x4>();
        public static Matrix4x4? ReadMatrix4x4Nullable(this NetworkReader reader) => reader.ReadBlittableNullable<Matrix4x4>();

        public static Guid ReadGuid(this NetworkReader reader) => new Guid(reader.ReadBytes(16));
        public static Guid? ReadGuidNullable(this NetworkReader reader) => reader.ReadBool() ? ReadGuid(reader) : default(Guid?);

        public static NetworkIdentity ReadNetworkIdentity(this NetworkReader reader)
        {
            uint netId = reader.ReadUInt();
            if (netId == 0)
                return null;

            // NOTE: a netId not being in spawned is common.
            // for example, "[SyncVar] NetworkIdentity target" netId would not
            // be known on client if the monster walks out of proximity for a
            // moment. no need to log any error or warning here.
            return Utils.GetSpawnedInServerOrClient(netId);
        }

        public static NetworkBehaviour ReadNetworkBehaviour(this NetworkReader reader)
        {
            // read netId first.
            //
            // IMPORTANT: if netId != 0, writer always writes componentIndex.
            //   reusing ReadNetworkIdentity() might return a null NetworkIdentity
            //   even if netId was != 0 but the identity disappeared on the client,
            //   resulting in unequal amounts of data being written / read.
            //   https://github.com/vis2k/Mirror/issues/2972
            uint netId = reader.ReadUInt();
            if (netId == 0)
                return null;

            // read component index in any case, BEFORE searching the spawned
            // NetworkIdentity by netId.
            byte componentIndex = reader.ReadByte();

            // NOTE: a netId not being in spawned is common.
            // for example, "[SyncVar] NetworkIdentity target" netId would not
            // be known on client if the monster walks out of proximity for a
            // moment. no need to log any error or warning here.
            NetworkIdentity identity = Utils.GetSpawnedInServerOrClient(netId);

            return identity != null
                   ? identity.NetworkBehaviours[componentIndex]
                   : null;
        }

        public static T ReadNetworkBehaviour<T>(this NetworkReader reader) where T : NetworkBehaviour
        {
            return reader.ReadNetworkBehaviour() as T;
        }

        public static NetworkBehaviour.NetworkBehaviourSyncVar ReadNetworkBehaviourSyncVar(this NetworkReader reader)
        {
            uint netId = reader.ReadUInt();
            byte componentIndex = default;

            // if netId is not 0, then index is also sent to read before returning
            if (netId != 0)
            {
                componentIndex = reader.ReadByte();
            }

            return new NetworkBehaviour.NetworkBehaviourSyncVar(netId, componentIndex);
        }

        public static Transform ReadTransform(this NetworkReader reader)
        {
            // Don't use null propagation here as it could lead to MissingReferenceException
            NetworkIdentity networkIdentity = reader.ReadNetworkIdentity();
            return networkIdentity != null ? networkIdentity.transform : null;
        }

        public static GameObject ReadGameObject(this NetworkReader reader)
        {
            // Don't use null propagation here as it could lead to MissingReferenceException
            NetworkIdentity networkIdentity = reader.ReadNetworkIdentity();
            return networkIdentity != null ? networkIdentity.gameObject : null;
        }

        public static List<T> ReadList<T>(this NetworkReader reader)
        {
            int length = reader.ReadInt();
            if (length < 0)
                return null;
            List<T> result = new List<T>(length);
            for (int i = 0; i < length; i++)
            {
                result.Add(reader.Read<T>());
            }
            return result;
        }

        public static T[] ReadArray<T>(this NetworkReader reader)
        {
            int length = reader.ReadInt();

            //  we write -1 for null
            if (length < 0)
                return null;

            // todo throw an exception for other negative values (we never write them, likely to be attacker)

            // this assumes that a reader for T reads at least 1 bytes
            // we can't know the exact size of T because it could have a user created reader
            // NOTE: don't add to length as it could overflow if value is int.max
            if (length > reader.Length - reader.Position)
            {
                throw new EndOfStreamException($"Received array that is too large: {length}");
            }

            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = reader.Read<T>();
            }
            return result;
        }

        public static Uri ReadUri(this NetworkReader reader)
        {
            string uriString = reader.ReadString();
            return (string.IsNullOrWhiteSpace(uriString) ? null : new Uri(uriString));
        }

        public static Texture2D ReadTexture2D(this NetworkReader reader)
        {
            Texture2D texture2D = new Texture2D(32, 32);
            texture2D.SetPixels32(reader.Read<Color32[]>());
            texture2D.Apply();
            return texture2D;
        }

        public static Sprite ReadSprite(this NetworkReader reader)
        {
            return Sprite.Create(reader.ReadTexture2D(), reader.ReadRect(), reader.ReadVector2());
        }
    }
}
