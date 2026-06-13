using System.Numerics;
using ServerCore.Model;

namespace WebStudyServer
{
    // 특정 모델을 변경하는 모든 것을 여기서 구현함.
    public class ManagerBase<T> where T : ModelBase
    {
        protected T _model;

        public ManagerBase(T model)
        {
            _model = model;
        }

        public T Model => _model;
    }
}
