using System;
using UnityEngine;
using UnityEngine.Rendering;

#pragma warning disable SA1402

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Abstract class for async callback request.
    /// </summary>
    internal abstract class AsyncCallbackRequest
    {
        /// <summary>
        /// Wait for completion.
        /// </summary>
        internal abstract void WaitForCompletion();
    }

    /// <summary>
    /// Request with result.
    /// </summary>
    /// <typeparam name="T">Result type.</typeparam>
    internal class ResultRequest<T> : AsyncCallbackRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultRequest{T}"/> class.
        /// </summary>
        /// <param name="result">Result value.</param>
        /// <param name="completion">Completion callback.</param>
        internal ResultRequest(T result, Action<T> completion)
        {
            completion?.Invoke(result);
        }

        /// <inheritdoc/>
        internal override void WaitForCompletion()
        {
            // Do nothing
        }
    }

    /// <summary>
    /// Request for GPU render texture readback.
    /// </summary>
    internal class TextureGPUReadbackRequest : AsyncCallbackRequest
    {
        private AsyncGPUReadbackRequest request;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureGPUReadbackRequest"/> class.
        /// </summary>
        /// <param name="renderTexture">Render texture to readback.</param>
        /// <param name="width">Width for the result texture.</param>
        /// <param name="height">Height for the result texture.</param>
        /// <param name="useMipmap">Whether to use mip map for the result texture.</param>
        /// <param name="completion">Completion callback.</param>
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

        /// <inheritdoc/>
        internal override void WaitForCompletion()
        {
            request.WaitForCompletion();
        }
    }

    /// <summary>
    /// Request for CPU render texture readback.
    /// </summary>
    internal class TextureCPUReadbackRequest : AsyncCallbackRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextureCPUReadbackRequest"/> class.
        /// </summary>
        /// <param name="renderTexture">Render texture to readback.</param>
        /// <param name="width">Width for the result texture.</param>
        /// <param name="height">Height for the result texture.</param>
        /// <param name="useMipmap">Whether to use mip map for the result texture.</param>
        /// <param name="completion">Completion callback.</param>
        internal TextureCPUReadbackRequest(RenderTexture renderTexture, int width, int height, bool useMipmap, Action<Texture2D> completion)
        {
            var result = new Texture2D(width, height, TextureFormat.RGBA32, useMipmap);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            completion?.Invoke(result);
        }

        /// <inheritdoc/>
        internal override void WaitForCompletion()
        {
            // Do nothing
        }
    }
}
