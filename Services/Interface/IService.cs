using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IService <TEntity> where TEntity : class
    {
        TEntity Find(object Id);

        IEnumerable<TEntity> GetAll();

        void Add(TEntity entity);

        void Update(TEntity entity);

        void Remove(TEntity entity);

        void Delete(object ID);

        int SaveChanges();
    }
}
