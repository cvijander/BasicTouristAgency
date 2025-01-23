namespace BasicTouristAgency.ViewModel
{
    public interface IPaginationViewModel<T> where T : class
    {
        public int TotalPages { get; }

        public int CurrentPage { get; set; }

        public bool HasPrevious { get; }

        public bool HasNext { get; }

        IEnumerable<T> Collection { get; set; }
    }
}
