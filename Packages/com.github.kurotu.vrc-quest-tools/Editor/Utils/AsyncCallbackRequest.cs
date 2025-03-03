using System;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Networking.UnityWebRequest;

namespace KRT.VRCQuestTools.Utils
{
    internal abstract class AsyncCallbackRequest
    {
        abstract internal void WaitForCompletion();
    }

    internal class ResultRequest<T> : AsyncCallbackRequest
    {
        private T result;
        private Action<T> completion;

        internal ResultRequest(T result, Action<T> completion)
        {
            this.result = result;
            this.completion = completion;
        }

        internal override void WaitForCompletion()
        {
            completion?.Invoke(result);
        }
    }

    internal class TextureGPUReadbackRequest : AsyncCallbackRequest
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
                result.Apply();
                completion?.Invoke(result);
            });
        }


        internal override void WaitForCompletion()
        {
            request.WaitForCompletion();
        }
    }

    internal class TextureCPUReadbackRequest : AsyncCallbackRequest
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
