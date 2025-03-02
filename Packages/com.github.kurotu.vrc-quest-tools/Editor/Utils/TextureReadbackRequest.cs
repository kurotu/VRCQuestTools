using System;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Networking.UnityWebRequest;

namespace KRT.VRCQuestTools.Utils
{
    internal abstract class TextureReadbackRequest
    {
        abstract internal void WaitForCompletion();
    }

    internal class ResultRequest<T> : TextureReadbackRequest
    {
        internal ResultRequest(T result, Action<T> completion)
        {
            completion?.Invoke(result);
        }

        internal override void WaitForCompletion()
        {
            // Do nothing
        }
    }

    internal class TextureResultReadbackRequest : TextureReadbackRequest
    {
        internal TextureResultReadbackRequest(Texture2D texture, Action<Texture2D> completion)
        {
            completion?.Invoke(texture);
        }
        internal override void WaitForCompletion()
        {
            // Do nothing
        }
    }

    internal class TextureGPUReadbackRequest : TextureReadbackRequest
    {
        private AsyncGPUReadbackRequest request;

        internal TextureGPUReadbackRequest(RenderTexture renderTexture, int width, int height, bool useMipmap, Action<Texture2D> completion)
        {
            request = AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGBA32, (req) =>
            {
                if (req.hasError)
                {
                    throw new InvalidOperationException("AsyncGPUReadback error");
                }
                var result = new Texture2D(width, height, TextureFormat.RGBA32, useMipmap);
                if (useMipmap)
                {
                    var mipmapCount = renderTexture.mipmapCount;
                    for (int i = 0; i < mipmapCount; i++)
                    {
                        using (var data = req.GetData<Color32>(i))
                        {
                            result.SetPixelData(data, i);
                        }
                    }
                }
                else
                {
                    using (var data = req.GetData<Color32>())
                    {
                        result.LoadRawTextureData(data);
                    }
                }
                completion?.Invoke(result);
            });
        }


        internal override void WaitForCompletion()
        {
            request.WaitForCompletion();
        }
    }

    internal class TextureCPUReadbackRequest : TextureReadbackRequest
    {
        internal TextureCPUReadbackRequest(RenderTexture renderTexture, int width, int height, bool useMipmap, Action<Texture2D> completion)
        {
            var result = new Texture2D(width, height, TextureFormat.RGBA32, useMipmap);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            completion?.Invoke(result);
        }

        internal override void WaitForCompletion()
        {
            // Do nothing
        }
    }
}
