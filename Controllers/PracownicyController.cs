using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
using SP_WorkTimeApp_Website.Data;
using SP_WorkTimeApp_Website.Models;

namespace SP_WorkTimeApp_Website.Controllers
{
    [Authorize]
    public class PracownicyController : Controller
    {
        private readonly ILogger<PracownicyController> _logger;
        private readonly UserManager<Administrator> _userManager;
        private readonly SignInManager<Administrator> _signInManager;
        private ApplicationDbContext myDbContext;

        public PracownicyController(ILogger<PracownicyController> logger,
            ApplicationDbContext context,
            SignInManager<Administrator> signInManager,
            UserManager<Administrator> userManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            myDbContext = context;
        }

        public async Task<IActionResult> Index()
        {
            List<Pracownik> model = await GetPracownicy();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int? pracownikId = 0, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Pracownicy");

            if (pracownikId == null || pracownikId == 0)
                return LocalRedirect(returnUrl);

            var _user = await _userManager.GetUserAsync(User);
            Usun(_user, pracownikId);
            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> Edytuj(int? id = 0, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Pracownicy");

            if (id == null || id == 0)
                return LocalRedirect(returnUrl);

            using(var context = myDbContext)
            {

                var model = context.Pracownicy.Where(x => x.Id == id).FirstOrDefault();
                var _user = await _userManager.GetUserAsync(User);
                var idFirmy = _user?.IdFirmy;
                if (model.IdFirmy != idFirmy)
                {
                    TempData["error"] = "Błąd: Nie znaleziono pracownika";

                    return LocalRedirect(returnUrl);
                }
                if (model == null)
                    return LocalRedirect(returnUrl);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edytuj(string? name, string? surname, string? stawka, int? nadgodziny, int? swiateczne, string? login, string? password1, string? password2, string submit, string? returnUrl = null, int? id = 0)
        {
            returnUrl ??= Url.Content("~/Pracownicy");

            if (id == null || id == 0)
            {
                TempData["error"] = "Błąd: Nie znaleziono pracownika";

                return LocalRedirect(returnUrl);
            }

            switch (submit)
            {
                case "change1":
                    if(!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(surname) && !string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(stawka))
                    {
                        using(var context = myDbContext)
                        {
                            var toEdit = context.Pracownicy.Where(x => x.Id == id).FirstOrDefault();
                            if (toEdit == null)
                            {
                                TempData["error"] = "Błąd: Nie znaleziono pracownika";

                                return LocalRedirect(returnUrl);
                            }

                            var _user = await _userManager.GetUserAsync(User);
                            var idFirmy = _user?.IdFirmy;
                            if(toEdit.IdFirmy != idFirmy)
                            {
                                TempData["error"] = "Błąd: Nie znaleziono pracownika";

                                return LocalRedirect(returnUrl);
                            }

                            stawka ??= "";
                            stawka = stawka.Replace('.', ',');
                            try
                            {
                                if (!string.IsNullOrEmpty(stawka))
                                {
                                    var test = float.Parse(stawka);
                                }
                            }
                            catch (Exception ex)
                            {
                                returnUrl = Url.Content("~/Pracownicy/Edytuj/" + id.ToString());
                                TempData["error"] = "Błąd: Niepoprawny format stawki (wymagana jest liczba)";

                                return LocalRedirect(returnUrl);
                            }

                            var s = float.Parse(stawka);

                            if (toEdit.FirstName != name)
                                toEdit.FirstName = name;
                            if (toEdit.LastName != surname)
                                toEdit.LastName = surname;
                            if (toEdit.StawkaPodstawowa != s)
                                toEdit.StawkaPodstawowa = s;
                            if (toEdit.Login != login)
                                toEdit.Login = login;
                            //if (toEdit.DataZatrudnienia.Year != dataZatrudnienia.Value.Year || toEdit.DataZatrudnienia.Month != dataZatrudnienia.Value.Month || toEdit.DataZatrudnienia.Day != dataZatrudnienia.Value.Day)
                            //    toEdit.DataZatrudnienia = dataZatrudnienia.Value;

                            if(nadgodziny < 100)
                            {
                                if(context.Ustawienia.Any(x => x.Name == "StawkaNadgodzin"))
                                {
                                    var stawkaNadgodziny = Int32.Parse(context.Ustawienia.FirstOrDefault(x => x.Name == "StawkaNadgodzin").Value);
                                    toEdit.StawkaNadgodzin = stawkaNadgodziny;
                                }
                                else
                                {
                                    var stawkaNadgodziny = 150;
                                    toEdit.StawkaNadgodzin = stawkaNadgodziny;
                                }
                            }
                            else
                            {
                                toEdit.StawkaNadgodzin = (int)nadgodziny;
                            }

                            if(swiateczne < 100)
                            {
                                if (context.Ustawienia.Any(x => x.Name == "StawkaSwiateczna"))
                                {
                                    var stawkaSwiateczna = Int32.Parse(context.Ustawienia.FirstOrDefault(x => x.Name == "StawkaSwiateczna").Value);
                                    toEdit.StawkaSwiateczna = stawkaSwiateczna;
                                }
                                else
                                {
                                    var stawkaSwiateczna = 200;
                                    toEdit.StawkaSwiateczna = stawkaSwiateczna;
                                }
                            }
                            else
                            {
                                toEdit.StawkaSwiateczna = (int)swiateczne;
                            }

                            context.Update(toEdit);
                            context.SaveChanges();
                        }
                        TempData["error"] = "Zmieniono dane";
                    }
                    else
                    {
                        //error
                        returnUrl = Url.Content("~/Pracownicy/Edytuj/" + id.ToString());

                        TempData["error"] = "Błąd: Nie uzupełniono wszystkich danych";
                    }
                    break;
                case "change2":
                    if(!string.IsNullOrEmpty(password1) && !string.IsNullOrEmpty(password2))
                    {
                        if(password1 == password2)
                        {
                            using (var context = myDbContext)
                            {
                                var toEdit = context.Pracownicy.Where(x => x.Id == id).FirstOrDefault();
                                if (toEdit == null)
                                    return LocalRedirect(returnUrl);

                                var _user = await _userManager.GetUserAsync(User);
                                var idFirmy = _user?.IdFirmy;
                                if (toEdit.IdFirmy != idFirmy)
                                {
                                    TempData["error"] = "Błąd: Nie znaleziono pracownika";

                                    return LocalRedirect(returnUrl);
                                }

                                byte[] data = System.Text.Encoding.ASCII.GetBytes(password1);
                                data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
                                String hash = System.Text.Encoding.ASCII.GetString(data);

                                if(toEdit.Password != hash)
                                    toEdit.Password = hash;

                                context.Update(toEdit);
                                context.SaveChanges();
                            }
                            TempData["error"] = "Zmieniono dane";
                        }
                        else
                        {
                            //error
                            returnUrl = Url.Content("~/Pracownicy/Edytuj/" + id.ToString());

                            TempData["error"] = "Błąd: Hasła nie są identyczne";
                        }
                    }
                    else
                    {
                        //error
                        returnUrl = Url.Content("~/Pracownicy/Edytuj/" + id.ToString());

                        TempData["error"] = "Błąd: Nie uzupełniono obu pól";
                    }
                    break;
                default:
                    //error
                    TempData["error"] = "Błąd: Nie znaleziono wybranej opcji";
                    break;
            }

            return LocalRedirect(returnUrl);
        }

        public IActionResult Dodaj()
        {
            List<Ustawienia> model = new List<Ustawienia>();
            using(var context = myDbContext)
            {
                if (context.Ustawienia.Any(x => x.Name == "StawkaNadgodzin"))
                    model.Add(context.Ustawienia.FirstOrDefault(x => x.Name == "StawkaNadgodzin"));
                if (context.Ustawienia.Any(x => x.Name == "StawkaSwiateczna"))
                    model.Add(context.Ustawienia.FirstOrDefault(x => x.Name == "StawkaSwiateczna"));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Dodaj(string name, string surname, string stawka, int nadgodziny, int swiateczne, string login, string password1, string password2, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Pracownicy");
            stawka ??= "";
            stawka = stawka.Replace('.', ',');
            bool correctType = true;
            try
            {
                if (!string.IsNullOrEmpty(stawka))
                {
                    var test = float.Parse(stawka);
                }
            } catch (Exception ex)
            {
                correctType = false;
            }
            if (!correctType)
            {
                //error
                returnUrl = Url.Content("~/Pracownicy/Dodaj");

                TempData["error"] = "Błąd: Niepoprawny format stawki (wymagana jest liczba)";
            }
            else if(!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(surname) && !string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(stawka) 
                && !string.IsNullOrEmpty(password1) && !string.IsNullOrEmpty(password2))
            {
                if(password1 == password2)
                {
                    using(var context = myDbContext)
                    {
                        Pracownik toAdd = new Pracownik();
                        toAdd.FirstName = name;
                        toAdd.LastName = surname;
                        toAdd.Login = login;
                        stawka = stawka.Replace('.', ',');
                        var s = float.Parse(stawka);
                        toAdd.StawkaPodstawowa = s;
                        toAdd.StawkaNadgodzin = nadgodziny;
                        toAdd.StawkaSwiateczna = swiateczne;
                        var _user = await _userManager.GetUserAsync(User);
                        var idFirmy = _user?.IdFirmy;
                        toAdd.IdFirmy = idFirmy;
                        //toAdd.DataZatrudnienia = dataZatrudnienia;

                        byte[] data = System.Text.Encoding.ASCII.GetBytes(password1);
                        data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
                        String hash = System.Text.Encoding.ASCII.GetString(data);

                        toAdd.Password = hash;
                        toAdd.CzyZatrudniony = true;
                        context.Pracownicy.Add(toAdd); 
                        context.SaveChanges();
                    }
                    TempData["error"] = "Dodano pracownika";
                }
                else
                {
                    //error
                    returnUrl = Url.Content("~/Pracownicy/Dodaj");

                    TempData["error"] = "Błąd: Hasła nie są identyczne";
                }
            }
            else
            {
                //error
                returnUrl = Url.Content("~/Pracownicy/Dodaj");

                TempData["error"] = "Błąd: Nie uzupełniono wszystkich danych";
            }
            return LocalRedirect(returnUrl);
        }

        private async void Usun(Administrator? _user, int? id = 0)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Błąd: Nie znaleziono pracownika";

                return;
            }  

            using (var context = myDbContext)
            {
                var toDelete = context.Pracownicy.Where(x => x.Id == id).FirstOrDefault();
                if (toDelete == null)
                {
                    TempData["error"] = "Błąd: Nie znaleziono pracownika";

                    return;
                }

                //var _user = await _userManager.GetUserAsync(User);
                var idFirmy = _user?.IdFirmy;
                if (toDelete.IdFirmy != idFirmy)
                {
                    TempData["error"] = "Błąd: Nie znaleziono pracownika";

                    return;
                }

                var raportsToDelete = context.Raporty.Where(x => x.PracownikId == id).ToList();

                if(raportsToDelete.Any())
                {
                    toDelete.CzyZatrudniony = false;
                    context.Update(toDelete);
                    context.SaveChanges();
                }
                else
                {
                    context.Pracownicy.Remove(toDelete);
                    context.SaveChanges();

                    TempData["error"] = "Pracownik został usunięty";
                }
            }
        }

        public async Task<List<Pracownik>> GetPracownicy()
        {
            List<Pracownik> pracownicy = new List<Pracownik>();

            using(var context = myDbContext)
            {
                var _user = await _userManager.GetUserAsync(User);
                var idFirmy = _user?.IdFirmy;
                var p = context.Pracownicy.Where(x => x.IdFirmy == idFirmy).ToList();
                foreach(var item in p)
                {
                    if(item.CzyZatrudniony)
                        pracownicy.Add(item);
                }
            }

            pracownicy = pracownicy.OrderBy(x => x.LastName).ToList();

            return pracownicy;
        }
    }
}
