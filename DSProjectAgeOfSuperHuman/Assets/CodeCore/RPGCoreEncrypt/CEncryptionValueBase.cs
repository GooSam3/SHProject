using UnityEngine;
using System.Collections.Generic;


using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

// 주의!  IOS 경우 아래 변수가 없으면 BinaryFormatter 컴파일 오류가 뜬다.
static public class GlobalIOSSerializer
{
    private static bool m_bChecked = false;
    static public void CheckIOS()
    {
        if (m_bChecked == false)
        {
            m_bChecked = true;
#if UNITY_IOS
                Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        }
    }
}


// 주의!! [케릭터 문자열의 경우 실제 문자열 길이와 BufferSize 가 같을 경우 헤쉬가 같으므로 좀 널널하기 수동조작할것
// 주의!! [케릭터 문자열을 암호화 할때는 BufferSize를 주의할것]
// 대칭키 기반의 간단한 메모리 암호화. 

public abstract class CEncryptionValueBase<TYPE> where TYPE : struct
{
    private   byte[]      m_aBufferValue;
    private   byte[]      m_aBufferKey;
    private   byte[]      m_aBufferExport;
    protected IntPtr      m_hNativeBuffer;
  
    private int m_iBufferSizeUse = 0;           public int p_iBuffserSizeUse { get { return m_iBufferSizeUse; } }
    private int m_iBufferTotalSize = 0;
    protected bool m_bValueLock = false;          //  Lock하면  Set 을 하지 않는다. 실수로 할당하는 것을 방지 

    protected CEncryptionValueBase(int iSize)
    {
        m_iBufferTotalSize = iSize;
        PrivEncryptAllocBufferSize(m_iBufferTotalSize);  
    }
    //-------------------------------------------------------------------
    public void SetValueLock(bool bLock)
    {
        m_bValueLock = bLock;
    }

    //--------------------------------------------------------------------
    private void PrivEncryptAllocBufferSize(int iSize)
    {
        m_aBufferKey = new byte[iSize];
        m_aBufferValue = new byte[iSize];
        m_aBufferExport = new byte[iSize];
        m_hNativeBuffer = Marshal.AllocHGlobal(iSize);

        PrivFillBufferRandom(m_aBufferKey, 0, iSize);
    }

    private void PrivEncryptFillFakeData()
    {
        PrivFillBufferRandom(m_aBufferValue, m_iBufferSizeUse, m_aBufferValue.Length);
    }

    private void PrivEncryptSymmeticalValue()
    {
        for(int i = 0; i < m_aBufferValue.Length; i++)
        {
            m_aBufferValue[i] = (byte)(m_aBufferValue[i]^m_aBufferKey[i]);
        }
    }
    
    private void PrivDecryptSymmeticalValue(int iStart, int iEnd)
    {
        if (iEnd > m_iBufferTotalSize)
        {
            //Error!!
            iEnd = m_iBufferTotalSize;
        }

        int iTotalLength = iStart + iEnd;
        for (int i = iStart; i < iTotalLength; i++)
        {
            m_aBufferExport[i] = (byte)(m_aBufferValue[i] ^ m_aBufferKey[i]);
        }
    }

    private void PrivFillBufferRandom(byte [] aBuffer, int iStartPosition, int iEnd)
    {
        if (iEnd > aBuffer.Length)
        {
            iEnd = aBuffer.Length;
        }

        for (int i = iStartPosition; i < iEnd; i++)
        {
                aBuffer[i] = (byte)UnityEngine.Random.Range(0, byte.MaxValue);
        }
    }

    //---------------------------------------------------------
    protected void ProtEncryptDataValueAdd(TYPE tValue)
    {
        int valueSize = Marshal.SizeOf(tValue);

        if (m_iBufferSizeUse + valueSize > m_aBufferValue.Length)
        {
            return;
        }
     
        Marshal.StructureToPtr(tValue, m_hNativeBuffer, false);
        Marshal.Copy(m_hNativeBuffer, m_aBufferValue, m_iBufferSizeUse, valueSize);
        m_iBufferSizeUse += valueSize;   
    }

    protected void ProtEncryptDataReset()
    {
        m_iBufferSizeUse = 0;
    }

    protected void ProtEncryptDataStart()
    {
        //남은 버퍼 공간을 페이크 데이터로 채운다.
        PrivEncryptFillFakeData();
        PrivEncryptSymmeticalValue();
    }

    protected TYPE ProtDecryptDataStart(int iStartPosition, int iDecryptSize)
    {
        PrivDecryptSymmeticalValue(iStartPosition, iDecryptSize);
        Marshal.Copy(m_aBufferExport, iStartPosition, m_hNativeBuffer, iDecryptSize);
        TYPE Data = (TYPE)Marshal.PtrToStructure(m_hNativeBuffer, typeof(TYPE));
        // 출력한 이후에 원본을 변조하여 값을 노출 시키지 않는다.
        PrivFillBufferRandom(m_aBufferExport, iStartPosition, iStartPosition + iDecryptSize);
        return Data;
    }
    //---------------------------------------------------------
}

public class EnInt : CEncryptionValueBase<int>
{
    public const byte DefaultCryptionBufferSize = 16; // 4바이트  int 4 개 규모
    public EnInt() : base(DefaultCryptionBufferSize) { Value = 0; }
    public EnInt(int iValue) : base(DefaultCryptionBufferSize)  { Value = iValue; }
    public int Value {
        set
        {
            if (m_bValueLock) return;

            ProtEncryptDataReset();
            ProtEncryptDataValueAdd(value);
            ProtEncryptDataStart();
        }
        get
        {
            return PrivDecryptDataInt();
        }
    }

    static public implicit operator int(EnInt pEnInt)
    {
        return pEnInt.Value;
    }
    //-----------------------------------------------------
    private int PrivDecryptDataInt()
    {
        return ProtDecryptDataStart(0, Marshal.SizeOf(typeof(int)));
    }
}

public class EnBool : CEncryptionValueBase<bool>
{
    public const byte DefaultCryptionBufferSize = 4; // 
    public EnBool() : base(DefaultCryptionBufferSize) { Value = false; }
    public EnBool(bool bValue) : base(DefaultCryptionBufferSize)  { Value = bValue; }

    public bool Value
    {
        set
        {
            if (m_bValueLock) return;

            ProtEncryptDataReset();
            ProtEncryptDataValueAdd(value);
            ProtEncryptDataStart();
        }
        get
        {
            return PrivDecryptDataBool();
        }
    }

    static public implicit operator bool(EnBool pEnInt)
    {
        return pEnInt.Value;
    }
    //-----------------------------------------------------
    private bool PrivDecryptDataBool()
    {
        return ProtDecryptDataStart(0, Marshal.SizeOf(typeof(bool)));
    }
}

public class EnFloat : CEncryptionValueBase<float>
{
    public const byte DefaultCryptionBufferSize = 16; // 4바이트  int 4 개 규모
    public EnFloat() : base(DefaultCryptionBufferSize) { Value = 0f; }
    public EnFloat(float fValue) : base(DefaultCryptionBufferSize)  { Value = fValue; }
    public float Value
    {
        set
        {
            if (m_bValueLock) return;

            ProtEncryptDataReset();
            ProtEncryptDataValueAdd(value);
            ProtEncryptDataStart();
        }
        get
        {
            return PrivDecryptDataFloat();
        }
    }

    private float PrivDecryptDataFloat()
    {
        return ProtDecryptDataStart(0, Marshal.SizeOf(typeof(float)));
    }

    static public implicit operator float(EnFloat pEnInt)
    {
        return pEnInt.Value;
    }
}

public class EnString : CEncryptionValueBase<char>
{
    public const byte DefaultCryptionBufferSize = 128; // 64개 문자갯수 
    private StringBuilder m_pStringNote;

    public EnString() : base(DefaultCryptionBufferSize)
    {
        m_pStringNote = new StringBuilder(DefaultCryptionBufferSize / 2);
    }
    public EnString(int iBufferSize) : base(iBufferSize)
    {
        m_pStringNote = new StringBuilder(iBufferSize / 2);
    }

    public EnString(string strText) : base(DefaultCryptionBufferSize)
    {
        m_pStringNote = new StringBuilder(DefaultCryptionBufferSize / 2);
        Value = strText;
    }

    public string Value
    {
        set
        {
            if (value == string.Empty) return;
            if (m_bValueLock) return;

            ProtEncryptDataReset();
            for (int i = 0; i < value.Length; i++)
            {
                ProtEncryptDataValueAdd(value[i]);
            }
            ProtEncryptDataStart();
        }
        get
        {
            return PrivDecryptDataString();
        }
    }

    static public implicit operator string(EnString pEnString)
    {
        return pEnString.Value;
    }

    private string PrivDecryptDataString()
    {
        int iCharSize = Marshal.SizeOf(typeof(char));
        for (int i = 0; i < p_iBuffserSizeUse; i += iCharSize)
        {
            char C = ProtDecryptDataStart(i, iCharSize);
            m_pStringNote.Append(C);
        }

        string strDecryptString = m_pStringNote.ToString();
        m_pStringNote.Length = 0;
        return strDecryptString;
    }
}