namespace SecureReository
{
    public class Simulation
    {

        public static void Main(String[] args)
        {

            int opcija = RegistracijaPrijava();
            //Prijava
            if (opcija == 1)
            {
                Console.WriteLine("Puna putanja sertifikata:");
                string pathToCertificate = null;
                string korisnickoime = "";
                string lozinka = "";

                if (pathToCertificate == null)
                {
                    pathToCertificate = Console.ReadLine();
                }
                if (Certificatecs.IsCertificateValid(pathToCertificate))
                {

                }
                else
                {
                    Console.WriteLine("Neispravan sertifikat");
                }
                List<User> users = User.GetUsers();


                Console.WriteLine("Korisnicko ime:");
                korisnickoime = Console.ReadLine();
                Console.WriteLine("Lozinka");
                lozinka = Console.ReadLine();
                User currentuser = User.FindUser(users, korisnickoime, lozinka);

                if (currentuser != null)
                {

                    Console.WriteLine("Preuzmi fajl 1");
                    Console.WriteLine("Dodaj fajl 2");
                    string op = Console.ReadLine();
                    if (op == "1")
                    {
                        Console.WriteLine("Lokacija za smjestanje fajla");
                        string lokacija = Console.ReadLine();
                        Console.WriteLine("Ime fajla");
                        string imeFajla = Console.ReadLine();
                        currentuser.KopirajFajl(imeFajla, lokacija, pathToCertificate);
                    }
                    else if (op == "2")
                    {

                        Console.WriteLine("apsolutan putanja fajla");
                        string path = Console.ReadLine();
                        currentuser.PreuzmiFajl(path, pathToCertificate);

                    }
                    else
                    {
                        Console.WriteLine("Greska.exit");
                    }
                }


            }
            //registracija
            else if (opcija == 0)
            {
                Console.WriteLine("korisnicko ime");
                string korisnickoIme = Console.ReadLine();
                Console.WriteLine("lozinka");
                string lozinka = Console.ReadLine();
                Console.WriteLine("email");
                string email = Console.ReadLine();
                Console.WriteLine("common name");
                string commonName = Console.ReadLine();
                Console.WriteLine("cert name");
                string certname = Console.ReadLine();
                User user = new User(korisnickoIme, lozinka, certname, commonName, email);

            }
            else { Console.WriteLine("Greska.exit"); }


        }
        public static int RegistracijaPrijava()
        {
            Console.WriteLine("Registracija 0");
            Console.WriteLine("Prijava 1");
            string op = Console.ReadLine();
            if (op == "1")
            {
                return 1;
            }
            else if (op == "0")
            {
                return 0;
            }
            else
            {
                Console.WriteLine("Nepostojeca opcija. exit");
                return 3;
            }

        }


    }
}
