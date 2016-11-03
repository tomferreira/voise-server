namespace Voise
{
    internal class ResponseCode
    {
        internal static readonly ResponseCode OK = new ResponseCode(200, "OK");
        internal static readonly ResponseCode ACCEPTED = new ResponseCode(201, "Accepted");
        internal static readonly ResponseCode NORESULT = new ResponseCode(202, "No result");
        internal static readonly ResponseCode ERROR = new ResponseCode(300);

        internal int Code { get; private set; }
        internal string Message { get; private set; }

        private ResponseCode(int code, string message = null)
        {
            Code = code;
            Message = message;
        }
    }
}
