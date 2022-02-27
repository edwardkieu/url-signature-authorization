namespace Common.Lib.Dtos
{
    public class DataResponse<T>
    {
        public int Status { get; set; }
        public T Data { get; set; }

        public DataResponse()
        {

        }

        public DataResponse(int status, T data)
        {
            Status = status;
            Data = data;
        }
    }
}