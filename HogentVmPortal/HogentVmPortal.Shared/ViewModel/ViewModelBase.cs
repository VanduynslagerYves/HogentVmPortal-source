namespace HogentVmPortal.Shared.ViewModel
{
    public interface ViewModelBase<R, T>
    {
        public abstract static R ToViewModel(T model);
    }
}
