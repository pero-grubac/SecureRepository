using System.Security.Cryptography;
using System.Text;

namespace SecureReository
{
    public class User
    {
        private static string DBDirestory = Path.Combine(Environment.CurrentDirectory, "DB");
        private static string DBUser = Path.Combine(Environment.CurrentDirectory, "Priprema", "DBUser.txt");

        private string korisnickoIme;
        private byte[] lozinka { get; set; }
        private byte[] salt { get; set; }
        public string getIme()
        {
            return korisnickoIme;
        }
        public User(string korisnickoIme, string lozinka, string certName, string commonName, string email)
        {
            this.korisnickoIme = korisnickoIme;
            this.salt = GenerateSalt();
            this.lozinka = Otisak(lozinka);
            string folderName = Path.Combine(DBDirestory, korisnickoIme);
            Certificatecs.GenerateCertificate(commonName, email, certName);

            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);

            string pathToCertificate = Path.Combine(Certificatecs.getUserCerts(), certName + ".pfx");
            Certificatecs.CreateKey(getFolderPath(), pathToCertificate);

            SaveUser(this.korisnickoIme, this.lozinka, this.salt);
        }

        public string getFolderPath()
        {
            return Path.Combine(DBDirestory, korisnickoIme);
        }
        private static void SaveUser(string korisnickoIme, byte[] lozinka, byte[] salt)
        {
            string lozinkaBase = Convert.ToBase64String(lozinka);
            string saltBase = Convert.ToBase64String(salt);
            using (StreamWriter sw = new StreamWriter(DBUser, append: true))
            {
                string line = $"{korisnickoIme},{lozinkaBase},{saltBase}";
                sw.WriteLine(line);
            }
        }
        public User(string korisnickoIme, byte[] lozinka, byte[] salt)
        {

            this.korisnickoIme = korisnickoIme;
            this.lozinka = lozinka;
            this.salt = salt;
        }

        private static int SaltLenght()
        {
            Random rand = new Random();
            int randomNumber = rand.Next(1, 101);
            return randomNumber;
        }
        private static byte[] GenerateSalt()
        {

            byte[] salt = new byte[SaltLenght()];
            var generator = new RNGCryptoServiceProvider();
            generator.GetNonZeroBytes(salt);
            return salt;
        }

        private byte[] Otisak(string password)
        {
            using SHA256 sHA256 = SHA256.Create();
            var bytesPass = Encoding.ASCII.GetBytes(password);
            byte[] result = bytesPass.Concat(this.salt).ToArray();
            byte[] passHash = sHA256.ComputeHash(result);
            return passHash;

        }
        public bool CheckHash(string input)
        {
            byte[] singedInput = Otisak(input);
            return singedInput.SequenceEqual(this.lozinka);
        }
        public static List<User> GetUsers()
        {
            List<User> users = new List<User>();


            string[] lines = File.ReadAllLines(DBUser);
            foreach (string line in lines)
            {

                string[] fields = line.Split(',');
                string korisnickoIme = fields[0];
                byte[] password = Convert.FromBase64String(fields[1]);
                byte[] salt = Convert.FromBase64String(fields[2]);
                User user = new User(korisnickoIme, password, salt);
                users.Add(user);
            }

            return users;
        }
        public static User FindUser(List<User> users, string korisnickoIme, string lozinka)
        {
            foreach (User user in users)
            {
                if (user.korisnickoIme == korisnickoIme && user.CheckHash(lozinka))
                {
                    return user;
                }
            }
            return null;
        }
        public void PreuzmiFajl(string path, string pathToCertificate)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            string dirPath = Path.Combine(getFolderPath(), fileName);
            if (!Directory.Exists(dirPath))
            {

                Directory.CreateDirectory(dirPath);
                Random rand = new Random();
                int parts = rand.Next(4, 10);
                long fileSize = new FileInfo(path).Length;
                long partSize = fileSize / parts;
                long lastPart = fileSize - (parts - 1) * partSize;
                // potpisi fajl 
                Certificatecs.Dgst(path, getFolderPath());
                string old = Path.Combine(getFolderPath(), fileName + ".dgst");
                string newPath = Path.Combine(dirPath, fileName + ".dgst");
                File.Move(old, newPath);



                using (var input = File.OpenRead(path))
                {
                    for (int i = 1; i <= parts; i++)
                    {
                        string dirName = Path.Combine(dirPath, $"{i + fileName}");
                        Directory.CreateDirectory(dirName);
                        string nameBaisc = $"{i + fileName + ".txt"}";
                        string partName = Path.Combine(dirName, nameBaisc);
                        long partToRead = (i == parts) ? lastPart : partSize;
                        using (var output = File.Create(partName))
                        {
                            byte[] buffer = new byte[1024];
                            long bytesRemaining = partToRead;
                            while (bytesRemaining > 0)
                            {
                                int readByte = input.Read(buffer, 0, (int)Math.Min(bytesRemaining, buffer.Length));
                                if (readByte == 0)
                                    break;
                                output.Write(buffer, 0, readByte);
                                bytesRemaining -= readByte;
                            }
                        }
                        // kriptuj taj fajl 

                        Certificatecs.CryptSym(getFolderPath(), partName, pathToCertificate);
                        string cryptname = "crypt" + nameBaisc;
                        string newPartCrypt = Path.Combine(dirName, cryptname);
                        string oldPartCrypt = Path.Combine(getFolderPath(), "kriptovan" + nameBaisc);
                        Console.WriteLine(oldPartCrypt);
                        Console.WriteLine(newPartCrypt);
                        File.Move(oldPartCrypt, newPartCrypt);
                        File.Delete(oldPartCrypt);
                    }
                }
            }
            else Console.WriteLine("Fajl vec postoji");

        }
        public void KopirajFajl(string name, string pathToCopy, string pathToCertificate)
        {

            string nameFile = Path.Combine(getFolderPath(), name);

            if (!Directory.Exists(nameFile))
            {
                Console.WriteLine("fajl ne postoji");
                return;
            }
            string newFileName = Path.Combine(nameFile, name + ".txt");
            File.Create(newFileName).Close();

            int numberParts = Directory.GetDirectories(nameFile).Length;
            for (int i = 1; i <= numberParts; i++)
            {
                string partFolderName = Path.Combine(nameFile, $"{i + name}");
                string partFilename = Path.Combine(partFolderName, $"{"crypt" + i + name + ".txt"}");
                // Console.WriteLine($"{partFilename}");
                Certificatecs.DecryptSym(getFolderPath(), partFilename, pathToCertificate);

                string newPartName = Path.Combine(getFolderPath(), $"{"dekriptovan" + "crypt" + i + name + ".txt"}");
                File.AppendAllText(newFileName, File.ReadAllText(newPartName));
                if (File.Exists(newPartName))
                {
                    File.Delete(newPartName);
                }


            }
            string oldDgst = Path.Combine(nameFile, name + ".dgst");
            bool dozvola = Certificatecs.Verify(newFileName, getFolderPath(), oldDgst);
            if (dozvola)
            {
                File.Copy(newFileName, Path.Combine(pathToCopy, name + ".txt"), true);
                //  File.SetAttributes(Path.Combine(pathToCopy, name + ".txt"), FileAttributes.Normal);
                File.Delete(newFileName);
            }
        }

    }
}
