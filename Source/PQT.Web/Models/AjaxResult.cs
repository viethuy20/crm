namespace PQT.Web.Models
{
    public class AjaxResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public AjaxResult()
        {
            IsSuccess = true;
        }
    }
}