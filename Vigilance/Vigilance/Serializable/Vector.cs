using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Vigilance.Serializable
{
    public struct Vector
    {
        public Vector(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector(Vector3 vector)
        {
            X = vector.x;
            Y = vector.y;
            Z = vector.z;
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        [YamlIgnore]
        public Vector3 ToVector3 => new Vector3(X, Y, Z);
        public static bool operator ==(Vector left, Vector right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        public static bool operator !=(Vector left, Vector right) => !(left == right);
        public static Vector operator *(Vector left, Vector right) => new Vector(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        public static Vector operator *(Vector left, float right) => new Vector(left.X * right, left.Y * right, left.Z * right);
        public static Vector operator *(float left, Vector right) => right * left;
        public static Vector operator /(Vector left, Vector right)
        {
            if (right.X == 0 || right.Y == 0 || right.Z == 0)
                throw new DivideByZeroException();

            return new Vector(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
        }

        public static Vector operator /(Vector left, float right)
        {
            if (right == 0)
                throw new DivideByZeroException();

            return new Vector(left.X / right, left.Y / right, left.Z / right);
        }

        public static Vector operator /(float left, Vector right) => right / left;
        public static Vector operator +(Vector left, Vector right) => new Vector(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        public static Vector operator -(Vector left, Vector right) => left + (-right);
        public static Vector operator -(Vector right) => new Vector(-right.X, -right.Y, -right.Z);
        public static bool operator >(Vector left, Vector right) => left.X > right.X && left.Y > right.Y && left.Z > right.Z;
        public static bool operator >=(Vector left, Vector right) => !(left < right);
        public static bool operator <(Vector left, Vector right) => left.X < right.X && left.Y < right.Y && left.Z < right.Z;
        public static bool operator <=(Vector left, Vector right) => !(left > right);
        public override bool Equals(object obj) => obj is Vector vector && X == vector.X && Y == vector.Y && Z == vector.Z && ToVector3.Equals(vector.ToVector3);

        public override int GetHashCode()
        {
            int hashCode = 1943678824;

            hashCode = (hashCode * -1521134295) + X.GetHashCode();
            hashCode = (hashCode * -1521134295) + Y.GetHashCode();
            hashCode = (hashCode * -1521134295) + Z.GetHashCode();
            hashCode = (hashCode * -1521134295) + ToVector3.GetHashCode();

            return hashCode;
        }

        public override string ToString() => $"[{X} | {Y} | {Z}]";
    }
}
