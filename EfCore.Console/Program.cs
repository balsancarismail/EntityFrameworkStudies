﻿// See https://aka.ms/new-console-template for more information


using AutoMapper.QueryableExtensions;
using EfCore.Console;
using EfCore.Console.Dal;
using EfCore.Console.Dtos;
using EfCore.Console.Mapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

using (var context = new AppDbContext())
{
    //AddEntity(context);
    //AddProduct(context);

    //FindEntitiesWithChangeTracker(context);

    //EagerLoading(context);
    //ExplicitLoading(context);

    //KeylessTables(context);

    //InnerJoingWithMethod(context);
    //InnerJoinWithLINQ(context);

    //LeftRightJoins(context);
    //OuterJoin(context);

    //RawSql(context);
    //await CustomSqlQueries(context);
    //await ToSqlQuery(context);

    //await Pagination(context, 1, 2);

    //IgnoreGlobalQueryFilter(context);

    //QueriesWithTagWith(context);

    //StoreProcedureBasick(context);
    //StoreProcedureWithParameters(context);
    //CustomStoreProcedureResult(context);

    //FunctionsWithParameters(context);
    //FunctionWithCsFunctionInDbContext(context);
    //RawSqlForFunctionReturnsScalarValue(context);

    //ProjectionEntity(context);
    //await ProjectionAnonymus(context);
    //ProjectionDto(context);
    //ProjectionWithProjectToMethodBestPractise(context);

    //TransactionManagement(context);
    //MultipleDbContextTransaction(context);


    //IsoloationLevel(context);

    Console.WriteLine("Hello World");

}


Console.WriteLine("Hello, World!");


/*
 * When we use Select, we dont need to Include or ThenInclude methods anymore. EfCore creates queries with automatically
 * and includes corresponding tables in joins
 */





static void AddEntity(AppDbContext context)
{
    context.Add<Teacher>(new Teacher { Name = "Ali", Phone = "123456789", Age = 32 });
    context.Add<Teacher>(new Teacher { Name = "Veli", Phone = "9874456321", Age = 42 });

    context.Add<Student>(new Student { Name = "Veli", StudentNumber = "9874456321", Age = 42 });
    context.Add<Student>(new Student { Name = "Veli", StudentNumber = "9874456321", Age = 42 });

    context.SaveChanges();
}

static void FindEntitiesWithChangeTracker(AppDbContext context)
{
    context.ChangeTracker.Entries().ToList().ForEach(entry =>
    {
        switch (entry.Entity)
        {
            case Teacher teacher:
                Console.WriteLine(teacher.Name + " " + teacher.Phone + " " + teacher.Age);
                break;

            case Student student:
                Console.WriteLine(student.Name + " " + student.StudentNumber + " " + student.Age);
                break;
            default:
                break;
        }
    });
}

static void EagerLoading(AppDbContext context)
{
    var result = context.Products.Include(p => p.ProductFeature).ThenInclude(pf => pf.Product).Include(p => p.Category).FirstOrDefault();
}

static void ExplicitLoading(AppDbContext context)
{
    var result = context.Categories.FirstOrDefault();
    context.Entry(result!).Collection(c => c.Products).Load();
}

static void AddProduct(AppDbContext context)
{
    context.Add<Product>(new Product { Name = "Kalem", Barcode = 122342, CategoryId = 1, DiscountPrice = 14, IsDeleted = false, Price = 234, Stock = 123 });
    context.Add<Product>(new Product { Name = "Kalem2", Barcode = 122124342, CategoryId = 1, DiscountPrice = 42, IsDeleted = false, Price = 2123, Stock = 1523 });
    context.Categories.Add(new Category { Name = "Kalemler" });

    context.SaveChanges();
}

static void KeylessTables(AppDbContext context)
{
    var result = context.FullProducts.FromSqlRaw(@"select p.Name , p.Price, p.Stock, p.DiscountPrice, c.Name as CategoryName from Products p
inner join Categories c
on p.CategoryId = c.Id").ToList();
}

static void InnerJoingWithMethod(AppDbContext context)
{
    var result = context.Categories.Join(context.Products, c => c.Id, p => p.CategoryId, (c, p) => new { c, p });
    var result2 = context.Categories.Join(context.Products, c => c.Id, p => p.CategoryId, (c, p) => new { c, p }).Join(context.ProductFeatures, a => a.p.Id, pf => pf.Id, (a, pf) => new { a, pf });
}

static void InnerJoinWithLINQ(AppDbContext context)
{
    var result = (from c in context.Categories
                  join p in context.Products on
                  c.Id equals p.CategoryId
                  select new { c, p }).ToList();

    var result2 = (from c in context.Categories
                   join p in context.Products on
                   c.Id equals p.CategoryId
                   join pf in context.ProductFeatures on
                   p.Id equals pf.Id
                   select new { c, p, pf }).ToList();
}

static void LeftRightJoins(AppDbContext context)
{
    var result = (from p in context.Products
                  join pf in context.ProductFeatures on
                  p.Id equals pf.Id into pfList
                  from pf in pfList.DefaultIfEmpty()
                  select new
                  {
                      ProductName = p.Name,
                      ProductPrice = p.Price,
                      ProductWidth = pf.Width,
                  }).ToList();
}

static void OuterJoin(AppDbContext context)
{
    var left = from p in context.Products
               join pf in context.ProductFeatures
               on p.Id equals pf.Id into pfList
               from pf in pfList.DefaultIfEmpty()
               select new
               {
                   ProductName = p.Name,
                   ProductPrice = p.Price,
                   ProductWidth = pf.Width,
               };
    var right = from pf in context.ProductFeatures
                join p in context.Products
                on pf.Id equals p.Id into pList
                from p in pList.DefaultIfEmpty()
                select new
                {
                    ProductName = p.Name,
                    ProductPrice = p.Price,
                    ProductWidth = pf.Width,
                };

    var outer = left.Union(right).ToList();
}

static async void  RawSql(AppDbContext context)
{
    var products = context.Products.FromSqlRaw("Select * From Products where id = {0}", 1).ToListAsync();
    var product = await context.Products.FromSqlInterpolated($"Select * From Products where id = {0}").ToListAsync();
}

static async Task CustomSqlQueries(AppDbContext context)
{
    var productEssentials = await context.ProductEssentials.FromSqlRaw("Select Id, Name, Price, DiscountPrice from Products").ToListAsync();
}

static async Task ToSqlQuery(AppDbContext context)
{
    var productEssemtials = await context.ProductEssentials.ToListAsync();
}

static async Task Pagination(AppDbContext context, int page, int size)
{
    var result = context.Products.Where(p => p.Price < 100).OrderByDescending(p => p.Id).Skip((page - 1) * size ).Take(size).ToList();
}

static void IgnoreGlobalQueryFilter(AppDbContext context)
{
    var products = context.Products.Where(p => p.Price < 100).IgnoreQueryFilters();
}

static void QueriesWithTagWith(AppDbContext context)
{
    var products = context.Products.TagWith("GetAll Products").Where(p => p.Name == "asd").ToArray();
}

static void StoreProcedureBasic(AppDbContext context)
{
    var productsWithProcedure = context.Products.FromSqlRaw("EXEC sp_read_products").ToList();
}

static void StoreProcedureWithParameters(AppDbContext context)
{
    var categoryId = 1;
    var price = 70;
    var FullProducts = context.FullProducts.FromSqlInterpolated($"Exec sp_read_all_products_with_join {categoryId},{price}").ToList();
}

static void CustomStoreProcedureResult(AppDbContext context)
{
    var sqlParamater = new SqlParameter("@newId", System.Data.SqlDbType.Int);
    sqlParamater.Direction = System.Data.ParameterDirection.Output;
    var product = new Product
    {
        Name = "Kalem6",
        Stock = 12,
        Price = 7,
        DiscountPrice = 6,
        CategoryId = 1,
        Barcode = 34565,
        IsDeleted = false
    };
    context.Database.ExecuteSqlInterpolated($"Exec sp_insert_product {product.Name},{product.Price},{product.DiscountPrice},{product.Stock},{product.Barcode},{product.CategoryId},{product.IsDeleted},{sqlParamater}  output");
}

static void FunctionsWithParameters(AppDbContext context)
{
    var category = 1;
    var fullProducts = context.Products.FromSqlInterpolated($"SELECT * FROM getFullProductsWithParameter({category})");
}

static void FunctionWithCsFunctionInDbContext(AppDbContext context)
{
    var fullProducts = context.FullProducts.ToArray();
}

static void RawSqlForFunctionReturnsScalarValue(AppDbContext context)
{
    int categoryId = 5;
    var productCount = context.ProductCount.FromSqlInterpolated($"SELECT dbo.getProductCount({categoryId}) as Count").ToList();
}

static void ProjectionEntity(AppDbContext context)
{
    var products = context.Products.ToList();
}

static async Task ProjectionAnonymus(AppDbContext context)
{
    var list = await context.Categories.Include(c => c.Products).ThenInclude(p => p.ProductFeature).Select(c => new
    {
        ProductNames = string.Join(",", c.Products.Select(p => p.Name)),
        CategoryName = c.Name,
        ProductCount = c.Products.Count(),
        ProductsPrice = c.Products.Sum(p => p.Price)
    }).Where(a => a.ProductCount > 3).OrderBy(a => a.ProductsPrice).ToListAsync();
}

static void ProjectionDto(AppDbContext context)
{
    var productDtos = context.Products.Select(p => new ProductDto
    {
        Price = p.Price,
        DiscountPrice = p.DiscountPrice,
        Stock = p.Stock,
        Id = p.Id,
        Name = p.Name
    }).Where(pdto => pdto.Price < 50).ToList();
}

static void ProjectionWithProjectToMethodBestPractise(AppDbContext context)
{
    var productDtoWithProjectionTo = context.Products.ProjectTo<ProductDto>(ObjectMapper.Lazy.ConfigurationProvider).Where(pdto => pdto.Price > 500).ToList();
}

static void TransactionManagement(AppDbContext context)
{
    using (var transaction = context.Database.BeginTransaction())
    {
        var category = new Category
        {
            Name = "Kitaplar"
        };

        context.Categories.Add(category);
        context.SaveChanges();

        var product = new Product
        {
            Name = "Kitap1",
            Stock = 2,
            Price = 10,
            DiscountPrice = 5,
            Barcode = 1234,
            IsDeleted = false,
            CategoryId = category.Id,
        };
        context.Products.Add(product);
        context.SaveChanges();
        transaction.Commit();
    }
}

static void MultipleDbContextTransaction(AppDbContext context)
{
    using (var transaction = context.Database.BeginTransaction())
    {
        var path = Initiliazer.Configuration.GetConnectionString("SqlServer");
        var connection = new SqlConnection(path);

        /*
         * Business Codes / Logic
         */

        context.SaveChanges();
        using (var context2 = new AppDbContext(connection))
        {

            context2.Database.UseTransaction(transaction.GetDbTransaction());

            /*
             * Business Codes / Logic
             */
            context2.SaveChanges();
        }

        transaction.Commit();
    }
}

static void IsoloationLevel(AppDbContext context)
{
    using (var transaction = context.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
    {
        /*
         * Transaction Levels can be given as a paramater
         * 
         * ReadUncommitted Level : There is no lock for table and rows 
         * - Transaction can read uncommitted raws
         * - While one transaction is reading data, another one can insert,update,delete
         * - Phantom and nonrepeatable problems
         * 
         * Reads commited data mentioned below
         * 
         * ReadCommitted Level: There is no lock for table and rows 
         * - Transaction can read commited raws
         * - While one transaction is reading data, another one can insert,update,delete
         * - Phantom and nonrepeatable problems
         * 
         * Repeatable Level: There is lock for range of transaction scope
         * - While one transaction is reading data, another one can not update and delete data, but can insert data
         * - Phantom problem
         * 
         * Serializable: There is lock for range of transaction scope
         * - While one transaction is reading data, another one can not update, delete and insert data
         * 
         * Snapshot: There is no lock for another transaction
         * - While one transaction is reading data, another one can update, delete and insert data
         * - Transaction provides consistent data during transaction scope, but other transaction can do operations at the same time
         * Sql Command : 
         * - alter database DATABASE_NAME
         * - set ALLOW_SNAPSHOT_ISOLATION on
         * 
         */
    }
}