// Thanks to Bryan Keiren (http://www.bryankeiren.com)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HeurekaGames.AssetHunter
{
    [System.Serializable]
    public class AssetHunterSerializableSystemType : IComparer<AssetHunterSerializableSystemType>
    {
        [SerializeField]
        private string m_Name;

        public string Name
        {
            get { return m_Name; }
        }

        [SerializeField]
        private string m_AssemblyQualifiedName;

        public string AssemblyQualifiedName
        {
            get { return m_AssemblyQualifiedName; }
        }

        [SerializeField]
        private string m_AssemblyName;

        public string AssemblyName
        {
            get { return m_AssemblyName; }
        }

        private System.Type m_SystemType;
        public System.Type SystemType
        {
            get
            {
                if (m_SystemType == null)
                {
                    GetSystemType();
                }
                return m_SystemType;
            }
        }

        private void GetSystemType()
        {
            m_SystemType = System.Type.GetType(m_AssemblyQualifiedName);
        }

        public AssetHunterSerializableSystemType(System.Type _SystemType)
        {
            m_SystemType = _SystemType;
            m_Name = _SystemType.Name;
            m_AssemblyQualifiedName = _SystemType.AssemblyQualifiedName;
            m_AssemblyName = _SystemType.Assembly.FullName;
        }

        public override bool Equals(System.Object obj)
        {
            AssetHunterSerializableSystemType temp = obj as AssetHunterSerializableSystemType;
            if ((object)temp == null)
            {
                return false;
            }
            return this.Equals(temp);
        }

        public override int GetHashCode()
        {
            return SystemType.GetHashCode();
        }

        public bool Equals(AssetHunterSerializableSystemType _Object)
        {
            return _Object.SystemType.Equals(SystemType);
        }

        public static bool operator ==(AssetHunterSerializableSystemType a, AssetHunterSerializableSystemType b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(AssetHunterSerializableSystemType a, AssetHunterSerializableSystemType b)
        {
            return !(a == b);
        }

        public int Compare(AssetHunterSerializableSystemType a, AssetHunterSerializableSystemType b)
        {
            if (a.SystemType.Name.CompareTo(b.SystemType.Name) != 0)
            {
                return a.SystemType.Name.CompareTo(b.SystemType.Name);
            }
            else
            {
                return 0;
            }
        }
    }

    public class SerializableSystemTypeComparer : IComparer<AssetHunterSerializableSystemType>
    {
        public int Compare(AssetHunterSerializableSystemType a, AssetHunterSerializableSystemType b)
        {
            if (a.SystemType.Name.CompareTo(b.SystemType.Name) != 0)
            {
                return a.SystemType.Name.CompareTo(b.SystemType.Name);
            }
            else
            {
                return 0;
            }
        }
    }
}