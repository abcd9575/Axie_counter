using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using WindowsFormsApp1.Database;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1
{
    static class 데이터베이스
    {
        private static DatabaseContext db { get; set; } = new DatabaseContext();
        public static DatabaseContext 접근 { get => db; }


        public static void 추가<T>(T 새로운데이터) where T : ModelBase
        {
            db.Entry(새로운데이터).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            db.SaveChanges();
        }
        public static async Task 추가Async<T>(T 새로운데이터) where T : ModelBase
        {
            db.Entry(새로운데이터).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await db.SaveChangesAsync();
        }

        public static void 무결성검사() // Db파일이 있는지 확인 후 없으면 자동 생성
        {
            if (db.Database.GetPendingMigrations().Any())
                db.Database.Migrate();
        }
    }
}
