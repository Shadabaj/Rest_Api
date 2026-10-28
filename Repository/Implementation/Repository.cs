using DAL;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Repository.Implementation
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected AppDbContext _db;

        public Repository(AppDbContext db)
        {
            _db = db;
        }

        public void Add(TEntity entity)
        {
            _db.Add(entity);
        }

        public void Delete(object ID)
        {
            TEntity entity=_db.Set<TEntity>().Find(ID);
            if (entity != null) 
            _db.Set<TEntity>().Remove(entity);
        }

        public TEntity Find(object Id)
        {
            return _db.Set<TEntity>().Find(Id);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _db.Set<TEntity>().ToList();
        }

        public void Remove(TEntity entity)
        {
            _db.Set<TEntity>().Remove(entity);
        }

        public int SaveChanges()
        {
            return _db.SaveChanges();
        }

        public void Update(TEntity entity)
        {
           _db.Set<TEntity>().Update(entity);
        }
    }
}
