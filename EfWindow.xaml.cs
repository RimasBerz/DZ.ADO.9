using Sales.EfContext;
using Sales.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sales
{
    /// <summary>
    /// Interaction logic for EFWindow.xaml
    /// </summary>
    public partial class EFWindow : Window
    {
        public EfContext.DataContext dataContext;
        public EFWindow()
        {
            InitializeComponent();
            dataContext = new();
            //ShowDepartmentsCount();
            //ShowProductsCount();
            //ShowManagersCount();
            //ShowSalesCount();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MonitorDepartments.Content =
                dataContext.Departments.Count();
            MonitorSales.Content =
                dataContext.Sales.Count();
            ShowDailyStatistics();
        }
        //private void ShowDepartmentsCount()
        //{
        //    MonitorDepartments.Content =
        //        dataContext.Departments.Count();
        //}
        //private void ShowManagersCount()
        //{
        //    MonitorManagers.Content =
        //        dataContext.Managers.Count();
        //}
        //private void ShowProductsCount()
        //{
        //    MonitorProducts.Content =
        //        dataContext.Products.Count();
        //}
        //private void ShowSalesCount()
        //{
        //    MonitorSales.Content =
        //        dataContext.Sales.Count();
        //}
        private void ShowDailyStatistics()
        {
            SalesCnt.Content = dataContext.Sales
                .Where(sale => sale.Moment.Date == DateTime.Now.Date).Count();
            SalesTotal.Content = dataContext.Sales
               .Where(sale => sale.Moment.Date == DateTime.Now.Date).Sum(sale => sale.Cnt);
            SalesMoney.Content = dataContext.Sales
                 .Where(sale => sale.Moment.Date == DateTime.Now.Date)
                .Join(dataContext.Products,
                s => s.ProductId, p => p.Id,
                (s, p) => s.Cnt * p.Price)
              .Sum()
              .ToString("0.00");

            SalesTopManager.Content = dataContext.Managers
                 .GroupJoin(
                dataContext.Sales.Where(s => s.Moment.Date == DateTime.Now.Date),
                    m => m.Id,
                    s => s.ManagerId,
                    (m, s) => new { Manager = m, Total = s.Sum(s => s.Cnt) })
                    .OrderByDescending(mix => mix.Total)
                    .Take(1)
                    .Select(mix => mix.Manager.ToShortString() + $"({mix.Total}")
                    .First();

            SalesTopProduct.Content = dataContext.Products
               .GroupJoin(
              dataContext.Sales.Where(s => s.Moment.Date == DateTime.Now.Date),
                  p => p.Id,
                  s => s.ProductId,
                  (p, ss) => new { Product = p, Total = p.Price * ss.Sum(s => s.Cnt ) })
                  .OrderByDescending(mix => mix.Total)
                  .Take(1)
                  .Select(mix => mix.Product.ToShortString() + $"({mix.Total}")
                  .First();

            SalesTopDepartmentP.Content = dataContext.Departments
                 .Join(dataContext.Managers, d => d.Id, m => m.Id_main_dep, (d, m) => new { Dep = d, Man = m })
                 .GroupJoin(
                     dataContext.Sales.Where(s => s.Moment.Date == DateTime.Now.Date),
                     dm => dm.Man.Id,
                     sale => sale.ManagerId,
                     (dm, sales) => new { Dep = dm.Dep, Man = dm.Man, Total = sales.Sum(sale => sale.Cnt) }
                 ).ToList()
                 .GroupBy(
                     dms => dms.Dep,
                     dms => dms.Total,
                     (dep, ts) => new { Dep = dep, Total = ts.Sum() }
                 )
                 .OrderByDescending(dt => dt.Total)
                 .Select(dt => dt.Dep.Name + $"({dt.Total})")
                 .First();


            // Суть ясна,но не в плане практики не очень понятно как работет подключение Join 
            //SalesTopDepartmentS.Content = dataContext.Departments
            //     .Join(dataContext.Managers, d => d.Id, m => m.Id_main_dep, (d, m) => new { Dep = d, Man = m })
            //     .Join(dataContext.Products, p1 => p1.Man, sales => sales.Id, (s, pp) => { Sale = s,Products = pp})
            //     .GroupJoin(
            //         dataContext.Sales.Where(s => s.Moment.Date == DateTime.Now.Date),

            //         dm => dm.Man.Id,
            //         sale => sale.ManagerId,
            //         (dm2, sales2) => new { Dep = dm2.Dep, Man = dm2.Man, Total = sales2.Join(dataContext.Products, p => p.Id, sales => sales.Id, (s,pp) => {Sale = s,Products = pp}) }
            //     ).ToList()
            //     .GroupBy(
            //         dms2 => dms2.Dep,
            //         dms2 => dms2.Total,
            //         (dep2, ts2) => new { Dep = dep2, Total = ts2.Sum() }
            //     )
            //     .OrderByDescending(dt => dt.Total)
            //     .Select(dt => dt.Dep.Name + $"({dt.Total})")
            //     .First();

        }

        private void AddSalesButton_Click(object sender, RoutedEventArgs e)
        {
            int managersCount = dataContext.Managers.Count();
            int productsCount = dataContext.Products.Count();
            for (int i = 0; i < 10; i++)
            {
                dataContext.Sales.Add(new()
                {
                    Id = Guid.NewGuid(),
                    ManagerId = dataContext.Managers
                                .Skip(App.random.Next(managersCount))
                                .First()
                                .Id,
                    ProductId = dataContext.Products
                                .Skip(App.random.Next(productsCount))
                                .First()
                                .Id,
                    Cnt = App.random.Next(1, 10),
                    Moment = DateTime.Now
                });
            }
            dataContext.SaveChanges();

                MonitorSales.Content = dataContext.Sales.Count();
            ShowDailyStatistics();
        }

    }
}
