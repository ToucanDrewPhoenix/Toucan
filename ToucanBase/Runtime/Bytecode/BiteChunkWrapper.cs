using Toucan.Runtime.Bytecode;

namespace Toucan.Runtime
{

public class ToucanChunkWrapper
{
    public BinaryChunk ChunkToWrap { get; set; }

    #region Public

    public ToucanChunkWrapper()
    {
        ChunkToWrap = null;
    }

    public ToucanChunkWrapper( BinaryChunk chunkToWrap )
    {
        ChunkToWrap = chunkToWrap;
    }

    #endregion
}

}
