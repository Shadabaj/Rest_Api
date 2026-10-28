using Repository.Interface;
using Services.Interface;


namespace Services.Implementation
{
    public class Service<TEntity> : IService<TEntity> where TEntity : class
    {

        IRepository<TEntity> _repository;

        public Service(IRepository<TEntity> repository)
        {
            _repository = repository;
        }

        public void Add(TEntity entity)
        {
            _repository.Add(entity);
        }

        public void Delete(object ID)
        {
            _repository.Delete(ID);
        }

        public TEntity Find(object Id)
        {
            return _repository.Find(Id);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _repository.GetAll();
        }

        public void Remove(TEntity entity)
        {
            _repository.Remove(entity);
        }

        public int SaveChanges()
        {
            return _repository.SaveChanges();
        }

        public void Update(TEntity entity)
        {
            _repository.Update(entity);
        }
    }
}
