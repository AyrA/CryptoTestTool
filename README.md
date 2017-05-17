# CryptoTestTool

This tool shows with a practical example how ransomware works

## CryptoTest Website

In the root folder of the repository is a website which can generate RSA keys and decrypt stuff.
You technically don't need it because the tool works fully offline,
apart from obtaining the master key. I host this site myself so you don't need to do it on your own.

Ransomware can work in two ways:

### Do everything offline

If ransomware operates like this, there is a chance to recover the keys until the application fully deleted it from memory.

### Use a C&C server

The second way is for a website to generate the keys. The private key is never sent to your machine.
You can launch the website in IIS and then do so by calling `Default.aspx?get=key`.
Ensure that `App_Data` is writeable because the site stores the master key there.

Use `?get=master` to obtain the master key or `?get=ABCD...` to get a key that has already been generated once.


## CryptoTestTool

This is the tool that will encrypt and decrypt your stuff.

To use it, put the exe and the dll into an empty directory and double click the .exe file.
It will guide you through the process and explains what it is doing.