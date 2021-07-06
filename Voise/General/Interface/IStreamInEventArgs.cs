namespace Voise.General.Interface
{
    public interface IStreamInEventArgs
    {
        byte[] Buffer { get; }


        int BytesStreamed { get; }
    }
}
