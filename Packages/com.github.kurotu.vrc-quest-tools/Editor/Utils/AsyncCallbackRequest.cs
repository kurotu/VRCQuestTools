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
    /// Request with void result.
    /// </summary>
    internal class ResultRequest : AsyncCallbackRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultRequest"/> class.
        /// </summary>
        /// <param name="completion">Completion callback.</param>
        internal ResultRequest(Action completion)
        {
            completion?.Invoke();
        }

        /// <inheritdoc/>
        internal override void WaitForCompletion()
        {
            // Do nothing
        }
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
        /// <param name="useMipmap">Whether to use mip map for the result texture.</param>
        /// <param name="makeNoLongerReadable">Make texture not readable.</param>
        /// <param name="completion">Completion callback.</param>
        internal TextureGPUReadbackRequest(RenderTexture renderTexture, bool useMipmap, bool makeNoLongerReadable, Action<Texture2D> completion)
        {
            var width = renderTexture.width;
            var height = renderTexture.height;
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
                result.Apply(true, makeNoLongerReadable);
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
        /// <param name="useMipmap">Whether to use mip map for the result texture.</param>
        /// <param name="makeNoLongerReadable">Make texture not readable.</param>
        /// <param name="completion">Completion callback.</param>
        internal TextureCPUReadbackRequest(RenderTexture renderTexture, bool useMipmap, bool makeNoLongerReadable, Action<Texture2D> completion)
        {
            int width = renderTexture.width;
            int height = renderTexture.height;
            var result = new Texture2D(width, height, TextureFormat.RGBA32, useMipmap);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply(true, makeNoLongerReadable);
            completion?.Invoke(result);
        }

        /// <inheritdoc/>
        internal override void WaitForCompletion()
        {
            // Do nothing
        }
    }
}
