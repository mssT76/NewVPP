using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewVPP.Models;
using NuGet.Protocol.Plugins;

namespace NewVPP.Extension
{
    public static class HashMD5
    {
        //Compute hash the string
        public static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            //chuyen hoa chuoi nhap vao thanh mang byte va tinh ham bam
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            //tao stringbuilder moi de gan mang byte va tao chuoi moi
            //
            var sBuilder = new StringBuilder();

            //vong lap for lap qua tung byte cua ham bam va
            //format tung cai thanh chuoi thap luc phan
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            //return chuoi thap luc phan 
            return sBuilder.ToString();
        }

        //verify hash va chuoi 
        public static bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash)
        {
            //hash chuoi nhap vao 
            var hashOfInput = GetHash(hashAlgorithm, input);

            //
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(hashOfInput, hash) == 0;
        }
    }
}

//public async Task<IActionResult> Register([Bind("UserName,Password", "ConfirmPassword")] Account model)
//{
//    if (ModelState.IsValid)
//    {
//        if (model.Password != model.ConfirmPassword)
//        {
//            ModelState.AddModelError(string.Empty, "Mat khau khong trung khop");
//            return View(model);
//        }
//        //Ma hoa mat khau
//        SHA256 hashMethod = SHA256.Create();
//        model.Password = Util.Cryptography.GetHash(hashMethod, model.Password);

//        _context.Add(model);
//        await _context.SaveChangesAsync();
//        return RedirectToAction(nameof(Login));
//    }
//    return View(model);
//}
//var loginUser = await _context.Account.FirstOrDefaultAsync(m => m.UserName == model.UserName);
//if (loginUser == null)
//{
//    ModelState.AddModelError("", "Dang nhap that bai");
//    return View(model);
//}
//else
//{
//    //Kiem tra xem ma md5 cua pass hien tai co khop voi md5 cua pass da luu k?
//    SHA256 hashMethod = SHA256.Create();
//    if (Util.Cryptography.VerifyHash(hashMethod, model.Password, loginUser.Password))
//    {
//        //Luu trang thai user
//        CurrentUser = loginUser.UserName;
//        return RedirectToAction("Index", "Home");
//    }
//    else
//    {
//        ModelState.AddModelError("", "Dang nhap that bai");
//        return View(model);
//    }
//}
//            }
//            return View(model);
//        }