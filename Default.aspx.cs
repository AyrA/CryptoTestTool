using System;
using System.IO;
using System.Web.UI;

public partial class _Default : Page
{
    private string KeyFile;

    private struct ApiResponse
    {
        public bool Success;
        public string Message;
        public object Data;
    }

    public _Default()
    {
        KeyFile = MP("App_Data/Master.bin");
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        var Res = new ApiResponse()
        {
            Success = false,
            Message = "Unspecified Error"
        };
        Response.ContentType = "application/json";
        Cryptic C = new Cryptic();
        if (!File.Exists(KeyFile))
        {
            C.CreateKey();
            File.WriteAllBytes(KeyFile, C.ExportKey(true));
        }
        else
        {
            C.ImportKey(File.ReadAllBytes(KeyFile));
        }

        byte[] Test = new byte[100];

        C.Decrypt(C.Crypt(Test));

        if (!string.IsNullOrEmpty(Request["get"]))
        {
            if (Request["get"] == "key")
            {
                var UserKey = new Cryptic();
                UserKey.CreateKey();
                //Encrypt private data
                byte[] PrivateData = C.Crypt(UserKey.ExportKey(true));
                //Public data for user
                byte[] PublicData = UserKey.ExportKey(false);

                File.WriteAllBytes(MP("App_Data/" + Cryptic.Hash(PublicData) + ".bin"), PrivateData);

                Res.Success = true;
                Res.Message = Cryptic.Hash(PublicData);
                Res.Data = PublicData;

                Response.Write(Res.ToJson());
                Response.End();
            }
            else if (Request["get"].IsAlphaNum())
            {
                if (Request["get"] == "master")
                {
                    Res.Message = "master key (public part only)";
                    Res.Data = C.ExportKey(false);
                    Res.Success = true;
                    Response.Write(Res.ToJson());
                    Response.End();
                }
                else
                {

                    var keyfile = MP("App_Data/" + Request["get"] + ".bin");
                    if (File.Exists(keyfile))
                    {
                        Cryptic KeyRequest = new Cryptic();
                        KeyRequest.ImportKey(C.Decrypt(File.ReadAllBytes(keyfile)));
                        Res.Success = true;
                        Res.Message = "Key found";
                        Res.Data = KeyRequest.ExportKey(false);
                        Response.Write(Res.ToJson());
                        Response.End();
                    }
                    Res.Message = "Key not found";
                    Response.Write(Res.ToJson());
                    Response.End();
                }
            }
            else
            {
                Res.Message = "Invalid request";
            }
        }
        else if (!string.IsNullOrEmpty(Request["decrypt"]))
        {
            var s = Request["decrypt"];
            var keyfile = "";
            byte[] body=new byte[0];
            if (Request.HttpMethod.ToLower() == "post")
            {
                using (var str = Request.GetBufferlessInputStream())
                {
                    body = str.ReadToEnd();
                }
            }

            if (body.Length == 0)
            {
                Res.Message = "No data to decrypt";
                Response.Write(Res.ToJson());
                Response.End();
            }
            else
            {
                if (s.IsAlphaNum())
                {
                    if (File.Exists(keyfile = MP("App_Data/" + s + ".bin")))
                    {
                        var Decryptor = new Cryptic();
                        Decryptor.ImportKey(C.Decrypt(File.ReadAllBytes(keyfile)));
                        Res.Data = Decryptor.Decrypt(body);
                        Res.Success = true;
                        Res.Message = "Decrypted file";
                        Response.Write(Res.ToJson());
                        Response.End();
                    }
                    else
                    {
                        Res.Message = "Invalid decrypt operation";
                        Response.Write(Res.ToJson());
                        Response.End();
                    }
                }
            }
        }
        Response.Write(Res.ToJson());
    }

    public string MP(string s)
    {
        return Server.MapPath(s);
    }
}