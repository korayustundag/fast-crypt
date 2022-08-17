using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Security.Cryptography;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace FastCrypt
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void btnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPassword.Password))
            {
                await ShowError("Info", "Please set a password...");
                return;
            }
            using (Aes aes = Aes.Create())
            {
                using (SHA256 sha = SHA256.Create())
                {
                    aes.Key = sha.ComputeHash(Encoding.UTF8.GetBytes(txtPassword.Password));
                }
                using (MD5 md = MD5.Create())
                {
                    aes.IV = md.ComputeHash(Encoding.UTF8.GetBytes(txtPassword.Password));
                }

                ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms,transform,CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            string data;
                            txtData.Document.GetText(Windows.UI.Text.TextGetOptions.None, out data);
                            sw.Write(data);
                        }
                        txtData.Document.SetText(Windows.UI.Text.TextSetOptions.None, Convert.ToBase64String(ms.ToArray()));
                    }
                }
            }
        }

        private async void btnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtPassword.Password))
                {
                    await ShowError("Info", "Please enter the password...");
                    return;
                }

                string data;
                txtData.Document.GetText(Windows.UI.Text.TextGetOptions.None, out data);

                if (!IsBase64String(data))
                {
                    await ShowError("Info", "No encrypted data found.");
                    return;
                }
                using (Aes aes = Aes.Create())
                {
                    using (SHA256 sha = SHA256.Create())
                    {
                        aes.Key = sha.ComputeHash(Encoding.UTF8.GetBytes(txtPassword.Password));
                    }
                    using (MD5 md = MD5.Create())
                    {
                        aes.IV = md.ComputeHash(Encoding.UTF8.GetBytes(txtPassword.Password));
                    }

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(data)))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                txtData.Document.SetText(Windows.UI.Text.TextSetOptions.None, sr.ReadToEnd());
                            }
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                await ShowError("Error", "An error occurred while decrypting.");
            }
            catch (Exception ex)
            {
                await ShowError("Error", "An unknown error has occurred.\nError Message: " + ex.Message);
            }
        }

        public bool IsBase64String(string base64)
        {
            base64 = base64.Trim();
            return (base64.Length % 4 == 0) && Regex.IsMatch(base64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }

        private async Task ShowError(string head, string msg)
        {
            ContentDialog errorDialog = new ContentDialog()
            {
                Title = head,
                Content = msg,
                PrimaryButtonText = "Ok"
            };

            await errorDialog.ShowAsync();
        }
    }
}
