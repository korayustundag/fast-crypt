using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace fast_crypt
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("(c) Koray Üstündağ. All rights reserved.");
            Console.WriteLine("Welcome to Fast Crypt");
            MainMenu();
        }

        static void MainMenu()
        {
            mainstart:
            Console.WriteLine();
            Console.WriteLine("Main Menu:");
            Console.WriteLine("1. Encrypt Text");
            Console.WriteLine("2. Decrypt Text");
            Console.WriteLine("3. Exit");
            Console.Write("Select Method: ");
            string? s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s))
            {
                err("The method cannot be left blank!");
                goto mainstart;
            }
            if (s == "1")
            {
            passstart:
                var pass = string.Empty;
                ConsoleKey key;
                Console.Write("Set a password:");
                do
                {
                    var keyInfo = Console.ReadKey(intercept: true);
                    key = keyInfo.Key;

                    if (key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        Console.Write("\b \b");
                        pass = pass[0..^1];
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        Console.Write("*");
                        pass += keyInfo.KeyChar;
                    }
                } while (key != ConsoleKey.Enter);
                if (string.IsNullOrWhiteSpace(pass))
                {
                    err("Please set a password!");
                    goto passstart;
                }
                Console.WriteLine();
            txtstart:
                Console.Write("Text: ");
                string? txt = Console.ReadLine();
                if (string.IsNullOrEmpty(txt))
                {
                    err("Please enter a text!");
                    goto txtstart;
                }
                Encrypt(pass, txt);
            }
            else if (s == "2")
            {
                var pass = string.Empty;
                ConsoleKey key;
                Console.Write("Enter a password:");
                do
                {
                    var keyInfo = Console.ReadKey(intercept: true);
                    key = keyInfo.Key;

                    if (key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        Console.Write("\b \b");
                        pass = pass[0..^1];
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        Console.Write("*");
                        pass += keyInfo.KeyChar;
                    }
                } while (key != ConsoleKey.Enter);
                if (string.IsNullOrWhiteSpace(pass))
                {
                    err("Please enter a password!");
                    MainMenu();
                }
                Console.WriteLine();
                Console.Write("Encrypted Text: ");
                string? txt = Console.ReadLine();
                if (string.IsNullOrEmpty(txt))
                {
                    err("Please enter a encrypted text!");
                    MainMenu();
                }
                if (!IsBase64String(txt))
                {
                    err("No encrypted data found.");
                    MainMenu();
                }
                Decrypt(pass, txt);
            }
            else if (s == "3")
            {
                Console.WriteLine("Goodbye...");
                Environment.Exit(0);
            }
            else
            {
                err("The method must be 1, 2 or 3!");
                goto mainstart;
            }
        }

        static void err(string msg)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        static bool IsBase64String(string base64)
        {
            base64 = base64.Trim();
            return (base64.Length % 4 == 0) && Regex.IsMatch(base64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }

        static void Encrypt(string pass, string txt)
        {
            using (Aes aes = Aes.Create())
            {
                using (SHA256 sha = SHA256.Create())
                {
                    aes.Key = sha.ComputeHash(Encoding.UTF8.GetBytes(pass));
                }
                using (MD5 md = MD5.Create())
                {
                    aes.IV = md.ComputeHash(Encoding.UTF8.GetBytes(pass));
                }

                ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(txt);
                        }
                        Console.WriteLine("-----Encrypted Text-----");
                        Console.WriteLine(Convert.ToBase64String(ms.ToArray()));
                        Console.WriteLine("-----Encrypted Text-----");
                    }
                }
            }
            MainMenu();
        }

        static void Decrypt(string pass, string txt)
        {
            using (Aes aes = Aes.Create())
            {
                using (SHA256 sha = SHA256.Create())
                {
                    aes.Key = sha.ComputeHash(Encoding.UTF8.GetBytes(pass));
                }
                using (MD5 md = MD5.Create())
                {
                    aes.IV = md.ComputeHash(Encoding.UTF8.GetBytes(pass));
                }

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(txt)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            Console.WriteLine("-----Decrypted Text-----");
                            Console.WriteLine(sr.ReadToEnd());
                            Console.WriteLine("-----Decrypted Text-----");
                        }
                    }
                }
            }
            MainMenu();
        }
    }
}