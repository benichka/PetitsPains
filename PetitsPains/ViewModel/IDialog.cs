namespace PetitsPains.ViewModel
{
    /// <summary>
    /// Interface that exposes properties needed for something that wants to behave like a dialog.
    /// </summary>
    public interface IDialog : IClosable
    {
        /// <summary>Result of a dialog box. May be null, true or false.</summary>
        bool? DialogResult { get; set; }
    }
}
