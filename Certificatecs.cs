using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SecureReository
{
    public class Certificatecs
    {
        private static string pathToPriprema = Path.Combine(Environment.CurrentDirectory, "Priprema");
        private static string pathToPrivate = Path.Combine(".", "Priprema", "private");
        private static string pathToCrlList = "crl\\crllist.crl";
        private static string pathtoCrlListDer = "crl\\crllistder.txt";
        private static string pathToRequests = "requests";
        private static string pathToNewCerts = Path.Combine(pathToPriprema, "newcerts");
        private static string pathToOpenssCnf = Path.Combine(".", "Priprema", "openssl.cnf");
        private static string pathToCrlNumber = Path.Combine(".", "Priprema", "crlnumber.txt");
        private static string pathToIndex = Path.Combine(".", "Priprema", "index.txt");

        private static string pathToDestkopCerts = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Certs");
        public static string getUserCerts()
        {
            return pathToDestkopCerts;
        }
        private static string pathToCerts = "certs";
        private static string pathToRsa = "private\\private4096.txt";
        private static string pathToRsaFolder = "private";
        private static string caName = "CACertDer.cert";
        private static string caNamePem = "CACert.cert";

        private static string configName = "openssl.cnf";

        // match
        private static string countryName = "BA";
        private static string stateName = "RS";
        private static string localityName = "Banja Luka";
        private static string organizationName = "Elektrotehnicki fakultet";
        private static string organizationalUnitName = "ETF";

        private static string passwordName = "sigurnost";

        //optional
        private static string emailAddressCA = "cacert@gmail.com";
        private static string commonNameCa = "CACert";

        private static string crlReason = "certificateHold";

        private static string pattern = @"\b\w+?,certificateHold\b";
        private static string patternForC = @"/C=(\w{2})/";
        private static string patternForSR = @"/ST=(\w{2})/";
        private static string patternForO = @"/O=([^/]+)/";
        private static string patternForOU = @"/OU=([^/]+)/";
        private static string patternForCN = @"/CN=([^/]+)/";
        private static string patternForEmail = @"/emailAddress=([^/]+)/";
        private static string patternForTime = @"\b\d{12}Z\b";
        private static string patternForFirstLetter = @"^V";
        private static string patternForCertificateDate = @"notAfter=(.*)";

        private static string nameOfKey = "kljuc.key";
        private static string nameOfPrivateKey = "privateKljuc.key";
        private static string nameOfPublicKey = "public.key";
        public static bool isCaValid()
        {
            string command = $"openssl verify -CAfile {caName} {caName} ";
            ProcessStartInfo procesInfo = new ProcessStartInfo("cmd.exe", "/C" + command);
            procesInfo.UseShellExecute = false;
            procesInfo.RedirectStandardOutput = true;
            procesInfo.WorkingDirectory = pathToPriprema;
            procesInfo.CreateNoWindow = true;
            procesInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process proces = new Process();
            proces.StartInfo = procesInfo;
            proces.Start();
            while (!proces.HasExited) ;

            string output = proces.StandardOutput.ReadToEnd();
            if (output.Contains("OK"))
                return true;
            return false;
        }



        // Napravi RSA kljuc
        private static void GenerateRSAFile(string rsaPath)
        {

            string command = $"openssl genrsa -out {rsaPath}";
            ExecuteProces(command, pathToPriprema);
        }
        public static void ExecuteProces(string command, string workingDirecotry, bool caPemNeeded = false)
        {
            if (caPemNeeded)
            {
                DerToPem(caName, caNamePem, pathToPriprema);
            }
            ProcessStartInfo procesInfo = new ProcessStartInfo("cmd.exe", "/C" + command);
            procesInfo.UseShellExecute = false;
            procesInfo.RedirectStandardOutput = true;
            procesInfo.WorkingDirectory = workingDirecotry;
            procesInfo.CreateNoWindow = true;
            procesInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process proces = new Process();
            proces.StartInfo = procesInfo;
            proces.Start();
            while (!proces.HasExited) ;

            if (caPemNeeded)
            {
                if (File.Exists(pathToPriprema + "\\" + caNamePem))
                    File.Delete(pathToPriprema + "\\" + caNamePem);
            }
        }
        private static void DerToPem(string fileDer, string filePem, string workingDirectory)
        {
            string command = $"openssl x509 -in {fileDer} -out {filePem} -inform der -outform pem";
            ExecuteProces(command, workingDirectory);
        }
        private static void PemToDer(string filePem, string fileDer, string workingDirectory)
        {
            string command = $"openssl x509 -in {fileDer} -out {filePem} -inform pem -outform der";
            ExecuteProces(command, workingDirectory);
        }
        public static void GenerateCaCertificate()
        {
            // Ako pravimo novi Ca napravicemo i novi kljuc
            // GenerateRSAFile();
            string command = $"openssl req -x509 -new  -days 3650" +
                             $" -subj \"/C={countryName}/ST={stateName}/L={localityName}/O={organizationName}/OU={organizationalUnitName}/CN={commonNameCa}/emailAddress={emailAddressCA}\"  " +
                             $"-outform der -out {caName}     -config {configName} -passout pass:{passwordName} ";

            ExecuteProces(command, pathToPriprema);


        }

        public static void GenerateCertificate(string commonName, string emailAddress, string certName)
        {

            string pathToUserRsa = CreatePathToFile(pathToRsaFolder, certName, ".txt");
            GenerateRSAFile(pathToUserRsa);

            string request = CreatePathToFile(pathToRequests, certName, ".req");
            string cert = CreatePathToFile(pathToCerts, certName, ".cert");
            string userCert = CreatePathToFile(pathToDestkopCerts, certName, ".pfx");
            // napravi zahtjev
            string command = $"openssl req  -new   -key {pathToUserRsa}  -out {request} " +
                             $" -subj \"/C={countryName}/ST={stateName}/L={localityName}/O={organizationName}/OU={organizationalUnitName}/CN={commonName}/emailAddress={emailAddress}\"  -passin pass:{passwordName} -config {configName} ";
            ExecuteProces(command, pathToPriprema);


            // odobri ga
            string command2 = $"openssl ca  -in {request}     -config {configName} -passin pass:{passwordName} -batch -out {cert}";
            ExecuteProces(command2, pathToPriprema, true);


            string pkcsFile = CreatePathToFile(pathToCerts, certName, ".pfx");
            ConvertToPKCS12(certName, pathToUserRsa);

            if (File.Exists(pathToPriprema + "\\" + pkcsFile))
            {
                File.Copy(pathToPriprema + "\\" + pkcsFile, userCert);

            }
        }

        private static void DelteAndMove(string certName)
        {
            if (File.Exists(pathToPriprema + "\\" + pathToRequests))
            {
                File.Delete(pathToPriprema + "\\" + pathToRequests);
            }

            if (File.Exists(pathToPriprema + "\\" + pathToCerts))
            {
                File.Move(pathToPriprema + "\\" + pathToCerts, pathToDestkopCerts + "\\" + certName + ".cert");

            }
            // Brisanje newcerts
            if (Directory.Exists(pathToPriprema))
            {
                string[] files = Directory.GetFiles(pathToNewCerts);

                foreach (string file in files)
                {

                    File.Delete(file);
                }
            }

        }
        public static void GenerateCrlLIst()
        {
            string command = $"openssl ca -gencrl -out {pathToCrlList} -config {configName} -passin pass:{passwordName}";
            ExecuteProces(command, pathToPriprema, true);
            CrlPemToDer(pathToCrlList, pathtoCrlListDer);
        }

        private static void CrlPemToDer(string CrlPem, string CrlDer)
        {
            string command = $"openssl crl -in {CrlPem} -out {CrlDer} -inform pem -outform der";
            ExecuteProces(command, pathToPriprema);
            if (File.Exists(CrlPem))
            {
                File.Delete(CrlPem);
            }
        }
        private static void CrlDerToPem(string CrlPem, string CrlDer)
        {
            string command = $"openssl crl -in {CrlDer} -out {CrlPem} -inform der -outform pem";
            ExecuteProces(command, pathToPriprema);
        }
        public static bool IsCertificateValid(string path)
        {
            string checkUser = CreatePathToFile(pathToCerts, "checkUser", ".cert");
            // pretvori pkcs u pem
            string comand1 = $"openssl pkcs12 -in \"{path}\" -nokeys -clcerts -out \"{checkUser}\" -passin pass:{passwordName}";
            ExecuteProces(comand1, pathToPriprema);

            string command2 = $"openssl x509 in {checkUser} -noout -subject";

            ProcessStartInfo procesInfo = new ProcessStartInfo("cmd.exe", "/C" + command2);
            procesInfo.UseShellExecute = false;
            procesInfo.RedirectStandardOutput = true;
            procesInfo.WorkingDirectory = pathToPriprema;
            procesInfo.CreateNoWindow = true;
            procesInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process proces = new Process();
            proces.StartInfo = procesInfo;
            proces.Start();
            while (!proces.HasExited) ;

            string output = proces.StandardOutput.ReadToEnd();
            string[] lines = File.ReadAllLines(pathToIndex);
            string matchLine = "";
            bool found = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(output))
                {
                    found = true;
                    matchLine = lines[i];
                    break;
                }
            }
            //ako ga nemam u index.txt
            if (!found)
                return false;
            // ako prvo slove nije V
            Regex firstLetter = new Regex(patternForFirstLetter);
            Match firstLetterMatch = firstLetter.Match(matchLine);
            if (!firstLetterMatch.Success)
                return false;

            /* string command3 = $"openssl x509 in {checkUser} -noout -enddate";
             ProcessStartInfo procesInfo2 = new ProcessStartInfo("cmd.exe", "/C" + command3);
             procesInfo2.UseShellExecute = false;
             procesInfo2.RedirectStandardOutput = true;
             procesInfo2.WorkingDirectory = pathToPriprema;
             procesInfo2.CreateNoWindow = true;
             procesInfo2.WindowStyle = ProcessWindowStyle.Hidden;
             proces.StartInfo = procesInfo2;
             proces.Start();
             while (!proces.HasExited) ;
             string outoutDate = proces.StandardOutput.ReadToEnd();

             // kreiraj vrijeme
             Match outputDateMatch = Regex.Match(outoutDate, patternForCertificateDate);
             string dateString = outputDateMatch.Groups[1].Value;
             DateTime certFormet = DateTime.ParseExact(dateString, "MMM  d HH:mm:ss yyyy 'GMT'", CultureInfo.InvariantCulture);
             DateTime newFotmat=DateTime.SpecifyKind(certFormet, DateTimeKind.Utc).ToLocalTime();
             DateTime formattedDate=DateTime.ParseExact(newFotmat.ToString("yyMMddHHmmss'Z'"), "yyMMddHHmmss'Z'",CultureInfo.InvariantCulture);
           */
            // da li je istekao
            Match vrijemeMatch = Regex.Match(matchLine, patternForTime);
            string vrijeme = vrijemeMatch.Value;
            DateTime date = DateTime.ParseExact(vrijeme, "yyMMddHHmmss'Z'", CultureInfo.InvariantCulture);

            string currentDate = DateTime.UtcNow.ToString("yyMMddHHmmss'Z'");
            DateTime currentDateTime = DateTime.ParseExact(currentDate, "yyMMddHHmmss'Z'", CultureInfo.InvariantCulture);
            int compareResult = DateTime.Compare(currentDateTime, date);
            if (compareResult > 0)
                return false;

            return true;
        }
        private static void ConvertToPKCS12(string name, string pathToUserRsa)
        {
            ;
            string pkcsFile = CreatePathToFile(pathToCerts, name, ".pfx");
            string userCert = CreatePathToFile(pathToCerts, name, ".cert");
            string command = $"openssl pkcs12 -export -out {pkcsFile} -inkey {pathToUserRsa} -in {userCert} -certfile {caNamePem} -passout pass:{passwordName}";
            ExecuteProces(command, pathToPriprema, true);
            /*  if (File.Exists(pathToUserRsa))
               {
                   File.Delete(pathToUserRsa);
               }
              */
        }
        private static string CreatePathToFile(string folder, string name, string extension)
        {
            return folder + "\\" + name + extension;
        }
        public static void RevokeCertificate(string certName)
        {
            string file = CreatePathToFile(pathToCerts, certName, ".cert");
            // DerToPem(file, file, workingDirectory);
            string command = $"openssl ca -revoke {file} -crl_reason {crlReason} -config {configName} -passin pass:{passwordName}";
            ExecuteProces(command, pathToPriprema, true);
            //  PemToDer(file, file, workingDirectory);
            GenerateCrlLIst();
        }
        public static void ReactivateCertificate(string commonName)
        {
            string[] lines = File.ReadAllLines(pathToIndex);
            int targetIndex = Array.FindIndex(lines, line =>
            line.Contains($"CN={commonName}", StringComparison.InvariantCultureIgnoreCase));
            if (targetIndex != -1)
            {
                string targetLine = lines[targetIndex];
                string output1 = Regex.Replace(targetLine, "^R", "V");
                string output = Regex.Replace(output1, pattern, "");
                lines[targetIndex] = output;
                //Console.WriteLine(output);

                File.WriteAllLines(pathToIndex, lines);
                GenerateCrlLIst();
            }

        }
        // i generise kljuc i potpise ga i obrise originalni
        public static void CreateKey(string path, string pathToCertificate)
        {
            string command = $"openssl rand -out {nameOfKey} 32";
            ExecuteProces(command, path);
            if (!File.Exists(path + nameOfPrivateKey))
                ExtractPrivateKey(pathToCertificate, path);
            if (!File.Exists(path + nameOfPublicKey))
                ExtractPublicKey(path);

            CryptAsym(path, nameOfKey, pathToCertificate);
        }
        public static void ExtractPublicKey(string path)
        {

            string command = $"openssl rsa -in \"{nameOfPrivateKey}\" -pubout -out \"{nameOfPublicKey}\" -passin pass:{passwordName}";
            ExecuteProces(command, path);
        }
        public static void ExtractPrivateKey(string pathCertificate, string path)
        {

            string command = $"openssl pkcs12 -in \"{pathCertificate}\" -nocerts -out \"{nameOfPrivateKey}\" -passin pass:{passwordName} -passout pass:{passwordName}";
            ExecuteProces(command, path);
        }
        // file sa ekstenziom
        public static void CryptAsym(string path, string file, string pathToCertificate)
        {
            if (!File.Exists(path + nameOfPrivateKey))
                ExtractPrivateKey(pathToCertificate, path);
            if (!File.Exists(path + nameOfPublicKey))
                ExtractPublicKey(path);

            string command = $"openssl pkeyutl -encrypt -pubin -inkey \"{nameOfPublicKey}\" -in \"{file}\" -out \"{file + ".crypt"}\"";
            ExecuteProces(command, path);
            string pathToFile = path + file;
            if (File.Exists(pathToFile))
            {
                File.Delete(pathToFile);
            }

        }
        public static void DecryptAsym(string path, string file, string pathToCertificate)
        {

            string fileName = Path.GetFileNameWithoutExtension(file);
            string fileBasic = Path.GetFileNameWithoutExtension(fileName);
            if (!File.Exists(Path.Combine(path, nameOfPrivateKey)))
                ExtractPrivateKey(pathToCertificate, path);

            string command = $"openssl pkeyutl -decrypt  -inkey \"{nameOfPrivateKey}\" -in \"{file}\" -out \"{fileBasic + ".dec"}\" -passin pass:{passwordName}";
            //Console.WriteLine(command);
            ExecuteProces(command, path);
        }
        // filename je puta putanja fajla,bice obrisan poslije toga
        public static void CryptSym(string path, string fileName, string pathToCertificate)
        {
            string priv = Path.Combine(path, nameOfPrivateKey);
            if (!File.Exists(priv))
                ExtractPrivateKey(pathToCertificate, priv);

            string cryptname = Path.GetFileNameWithoutExtension(fileName);

            string deckey = Path.GetFileNameWithoutExtension(nameOfKey) + ".key";
            string decpath = Path.Combine(path, deckey);

            if (!File.Exists(decpath))
                DecryptAsym(path, nameOfKey, pathToCertificate);

            string command = $"openssl enc -aes-256-cbc -in \"{fileName}\" -out {"kriptovan" + cryptname + ".txt"} -pass file:{deckey} ";
            //  Console.WriteLine(command);
            ExecuteProces(command, path);

            if (File.Exists(fileName))
                File.Delete(fileName);

        }
        // file name je puna putanja fajla, moze biti obrisan ali je zakomentarisano
        public static void DecryptSym(string path, string fileName, string pathToCertificate)
        {
            if (!File.Exists(Path.Combine(path, pathToPrivate)))
                ExtractPrivateKey(pathToCertificate, path);



            string deckey = Path.GetFileNameWithoutExtension(nameOfKey) + ".key";
            string decpath = Path.Combine(path, nameOfKey);

            if (!File.Exists(decpath))
                DecryptAsym(path, nameOfKey, pathToCertificate);

            string fileNewName = Path.GetFileNameWithoutExtension(fileName);
            string command = $"openssl enc -d -aes-256-cbc -in \"{fileName}\" -out {"dekriptovan" + fileNewName + ".txt"} -pass file:{nameOfKey} ";

            ExecuteProces(command, path);
            /*  if (!File.Exists(fileName))
                  File.Delete(fileName);
            */
        }

        // puna putanja fajla 
        public static void Dgst(string nameFile, string path)
        {
            string fileBasic = Path.GetFileNameWithoutExtension(nameFile);

            string command = $"openssl dgst -sha256 -sign \"{nameOfPrivateKey}\" -passin pass:{passwordName} -out \"{fileBasic + ".dgst"}\" \"{nameFile}\"";


            ExecuteProces(command, path);

        }
        public static bool Verify(string nameFile, string pathToPublic, string pathOfSigned)
        {
            string command = $"openssl dgst -sha256 -verify \"{pathToPublic + "\\" + nameOfPublicKey}\" -passin pass:{passwordName} -signature \"{pathOfSigned}\" \"{nameFile}\"";
            ProcessStartInfo procesInfo = new ProcessStartInfo("cmd.exe", "/C" + command);
            procesInfo.UseShellExecute = false;
            procesInfo.RedirectStandardOutput = true;
            procesInfo.WorkingDirectory = pathToPublic;
            procesInfo.CreateNoWindow = true;
            procesInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process proces = new Process();
            proces.StartInfo = procesInfo;
            proces.Start();
            while (!proces.HasExited) ;

            string output = proces.StandardOutput.ReadToEnd();
            if ("Verified OK\n".Equals(output))
                return true;
            return false;
        }
    }
}
