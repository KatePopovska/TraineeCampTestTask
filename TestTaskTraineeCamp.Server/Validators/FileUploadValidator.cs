using FluentValidation;

namespace TestTaskTraineeCamp.Server.Validators
{
    public class FileUploadValidator : AbstractValidator<IFormFile>
    {
        public FileUploadValidator()
        {
            RuleFor(x => x.FileName).Must(ValidateFileExtention).WithMessage("Only .docx files are allowed.");
        }

        private bool ValidateFileExtention(string fileName)
        {
            return Path.GetExtension(fileName).Equals(".docx", StringComparison.OrdinalIgnoreCase);
        }
    }
}
