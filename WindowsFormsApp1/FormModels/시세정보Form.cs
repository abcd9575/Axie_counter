using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.FormModels
{
    public class 시세정보Form
    {
        public ServerType 서버 { get; set; }
        public string 아이템이름 { get; set; }
        public decimal 시세가 { get; set; }
        public List<ItemOption> 옵션정보 { get; set; } = new List<ItemOption>();

        public async Task 저장() {
            var db = 데이터베이스.접근;
            using (var Transaction = db.Database.BeginTransaction()) {
                try {
                    var newEntry = new PriceInfo
                    {
                        Server = 서버,
                        ItemName = 아이템이름,
                        Price = 시세가
                    };
                    await 데이터베이스.추가Async(newEntry);

                    foreach (var 옵션 in 옵션정보) {
                        var Existing = (from io in db.ItemOptions
                                        where io.Stat == 옵션.Stat && io.Value == 옵션.Value
                                        select io).SingleOrDefault();
                        if (Existing != null)
                            옵션.Id = Existing.Id;
                        else {
                            await db.ItemOptions.AddAsync(옵션);
                            await db.SaveChangesAsync();
                        }

                        db.PriceInfoItemOption.Add(new PriceInfo2ItemOption
                        {
                            PriceInfoId = newEntry.Id,
                            ItemOptionId = 옵션.Id
                        });
                        await db.SaveChangesAsync();
                    }

                    await db.SaveChangesAsync();
                    await Transaction.CommitAsync();
                }
                catch (Exception ex) {
                    await Transaction.RollbackAsync();
                    throw ex;
                }
            }
        }
        public IList<PriceInfo> 조회() {
            var 옵션정보 = this.옵션정보;
            var db = 데이터베이스.접근;


            IQueryable<PriceInfo2ItemOption> QuerySet= db.PriceInfoItemOption
                                                .Include(entry => entry.ItemOption)
                                                .Include(entry => entry.PriceInfo);
            if (this.시세가 > 0) {
                QuerySet = (from q in QuerySet
                            join pi in db.PriceInfo on q.PriceInfoId equals pi.Id
                            where pi.ItemName == 아이템이름 && pi.Server == 서버 && pi.CreatedAt >= DateTime.Today && pi.Price == this.시세가
                            select q);
            }
            else {
                QuerySet = (from q in QuerySet
                            join pi in db.PriceInfo on q.PriceInfoId equals pi.Id
                            where pi.ItemName == 아이템이름 && pi.Server == 서버 && pi.CreatedAt >= DateTime.Today
                            select q);
            }

            if (this.옵션정보.Any()) {
                var ItemOptions = 옵션정보.Where(io => io.Value > 0)
                                .SelectMany(option => (from io in db.ItemOptions
                                                       where io.Stat == option.Stat && io.Value == option.Value
                                                       select io.Id)).Distinct().ToList();
                var Filtered = (from io in db.ItemOptions
                                where ItemOptions.Contains(io.Id)
                                select io).Distinct();

                QuerySet = (from q in QuerySet
                            join io in Filtered on q.ItemOptionId equals io.Id
                            select q);
                
            }
            var TargetDataSet = db.PriceInfo
                                    .Include(pi => pi.PriceInfo2ItemOptions)
                                    .Include("PriceInfo2ItemOptions.ItemOption");

            var Result = (from q in QuerySet
                          join pi in TargetDataSet on q.PriceInfoId equals pi.Id
                          select pi).Distinct().ToList();
            return Result;
        }
    }
}
