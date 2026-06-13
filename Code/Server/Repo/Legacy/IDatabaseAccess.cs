using ServerCore.Model;

namespace WebStudyServer.Repo
{
    public interface IDatabaseAccess<TModel, TKey> where TModel : ModelBase, new()
    {
        IEnumerable<TModel> GetList();

        bool TryGetById(ulong id, out TModel model);

        bool TryGetByKey(TKey key, out TModel model);

        void Update(TModel entity);

        void Create(TModel entity);
    }
}
