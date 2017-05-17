using System;
using System.Security.Cryptography;
using System.IO;

public class Cryptic
{
    private RSAParameters Params;
    public bool HasPrivate { get; private set; }

    public Cryptic()
    {
        RSACryptoServiceProvider.UseMachineKeyStore = true;
    }

    public void ImportKey(byte[] Data)
    {
        Params = new RSAParameters();
        using (MemoryStream MS = new MemoryStream(Data, false))
        {
            using (BinaryReader BR = new BinaryReader(MS))
            {
                Params.Modulus = BR.ReadBytes(BR.ReadInt32());
                Params.Exponent = BR.ReadBytes(BR.ReadInt32());
                //Has private part
                HasPrivate = MS.Position < MS.Length;
                if (HasPrivate)
                {
                    Params.D = BR.ReadBytes(BR.ReadInt32());
                    Params.P = BR.ReadBytes(BR.ReadInt32());
                    Params.Q = BR.ReadBytes(BR.ReadInt32());
                    Params.DP = BR.ReadBytes(BR.ReadInt32());
                    Params.DQ = BR.ReadBytes(BR.ReadInt32());
                    Params.InverseQ = BR.ReadBytes(BR.ReadInt32());
                }
            }
        }
    }

    public void CreateKey()
    {
        using (RSACryptoServiceProvider R = new RSACryptoServiceProvider())
        {
            R.PersistKeyInCsp = false;
            Params = R.ExportParameters(true);
            R.Clear();
        }
        HasPrivate = true;
    }

    public byte[] ExportKey(bool IncludePrivate = false)
    {
        using (MemoryStream MS = new MemoryStream())
        {
            using (BinaryWriter BW = new BinaryWriter(MS))
            {
                WB(BW, Params.Modulus);
                WB(BW, Params.Exponent);
                if (IncludePrivate)
                {
                    WB(BW, Params.D);
                    WB(BW, Params.P);
                    WB(BW, Params.Q);
                    WB(BW, Params.DP);
                    WB(BW, Params.DQ);
                    WB(BW, Params.InverseQ);
                }
                BW.Flush();
                return MS.ToArray();
            }
        }
    }

    private static void WB(BinaryWriter BW, byte[] Data)
    {
        BW.Write(Data.Length);
        BW.Write(Data);
    }

    public byte[] Crypt(byte[] Source)
    {
        using (RSACryptoServiceProvider R = new RSACryptoServiceProvider())
        {
            R.PersistKeyInCsp = false;
            R.ImportParameters(Params);
            using (RijndaelManaged RM = new RijndaelManaged())
            {
                RM.GenerateIV();
                RM.GenerateKey();
                byte[] EIV = R.Encrypt(RM.IV, true);
                byte[] EPW = R.Encrypt(RM.Key, true);
                using (RijndaelManagedTransform Enc = (RijndaelManagedTransform)RM.CreateEncryptor())
                {
                    using (MemoryStream MS = new MemoryStream())
                    {
                        using (BinaryWriter BW = new BinaryWriter(MS))
                        {
                            WB(BW, EIV);
                            WB(BW, EPW);
                            BW.Flush();
                            using (CryptoStream CS = new CryptoStream(MS, Enc, CryptoStreamMode.Write))
                            {
                                CS.Write(Source, 0, Source.Length);
                                CS.FlushFinalBlock();
                            }
                            return MS.ToArray();
                        }
                    }
                }
            }
        }
    }

    public byte[] Decrypt(byte[] Data)
    {
        if (!HasPrivate)
        {
            throw new Exception("Can't decrypt without private key!");
        }
        using (RSACryptoServiceProvider R = new RSACryptoServiceProvider())
        {
            R.PersistKeyInCsp = false;
            R.ImportParameters(Params);
            using (RijndaelManaged RM = new RijndaelManaged())
            {
                using (MemoryStream MS = new MemoryStream(Data, false))
                {
                    using (BinaryReader BR = new BinaryReader(MS))
                    {
                        RM.IV = R.Decrypt(BR.ReadBytes(BR.ReadInt32()), true);
                        RM.Key = R.Decrypt(BR.ReadBytes(BR.ReadInt32()), true);
                        using (RijndaelManagedTransform Dec = (RijndaelManagedTransform)RM.CreateDecryptor())
                        {
                            using (CryptoStream CS = new CryptoStream(MS, Dec, CryptoStreamMode.Read))
                            {
                                return CS.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
    }

    public static string Hash(byte[] Data)
    {
        using (var Hasher = new SHA1Managed())
        {
            return CryptoTest.Converter.FromBytes(Hasher.ComputeHash(Data));
        }
    }

}
