using Intel.RealSense;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

public class MyTextureRenderer : MonoBehaviour
{
    public struct StreamBuffer
    {
        public IntPtr pointer;
        public int width;
        public int height;
    }

    private FrameQueue _threadSafeQueue;
    private StreamBuffer _buffer;
    private IntPtr _localBuffer = IntPtr.Zero;
    private int _bufferSize = 0;

    public RsFrameProvider _source;
    public Stream _desiredRSStream;
    public Format _desiredRSFormat;
    public int _streamIndex = -1;

    void Start()
    {
        _source.OnStart += OnStartStreaming;
        _source.OnStop += OnStopStreaming;
    }

    void OnDestroy()
    {
        if (_threadSafeQueue != null)
        {
            _threadSafeQueue.Dispose();
        }

        FreeBuffer();
    }

    private void FreeBuffer()
    {
        if (_localBuffer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_localBuffer);
            _localBuffer = IntPtr.Zero;
            _bufferSize = 0;
        }
    }

    // Expose the protected texture
    public StreamBuffer GetTexture()
    {
        return _buffer; // 'texture' is protected in the base class
    }

    protected void OnStopStreaming()
    {
        _source.OnNewSample -= OnNewSample;
        if (_threadSafeQueue != null)
        {
            _threadSafeQueue.Dispose();
            _threadSafeQueue = null;
        }
    }

    public void OnStartStreaming(PipelineProfile activeProfile)
    {
        _threadSafeQueue = new FrameQueue(1);
        _source.OnNewSample += OnNewSample;
    }

    void OnNewSample(Frame frame)
    {
        try
        {
            if (frame.IsComposite)
            {
                using (var fs = frame.As<FrameSet>())
                {
                    foreach (var f in fs)
                    {
                        using (f)
                        using (var p = f.Profile)
                        {
                            if (p.Stream == _desiredRSStream && p.Format == _desiredRSFormat && (p.Index == _streamIndex || _streamIndex == -1))
                            {
                                if (f != null)
                                    _threadSafeQueue.Enqueue(f);
                                return;
                            }
                        }
                    }
                }
            }

            using (frame)
            {
                _threadSafeQueue.Enqueue(frame);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            // throw;
        }

    }

    protected void LateUpdate()
    {
        if (_threadSafeQueue != null)
        {
            VideoFrame frame;
            if (_threadSafeQueue.PollForFrame<VideoFrame>(out frame))
                using (frame)
                    ProcessFrame(frame);
        }
    }

    private void ProcessFrame(VideoFrame frame)
    {
        int bytesPerPixel = (frame.Profile.Format == Format.Z16) ? 2 : 1; // Depth = 2 bytes, Infrared = 1 byte
        int requiredSize = frame.Width * frame.Height * bytesPerPixel;

        // Allocate or reallocate buffer if needed
        if (_localBuffer == IntPtr.Zero || _bufferSize < requiredSize)
        {
            FreeBuffer();
            _localBuffer = Marshal.AllocHGlobal(requiredSize);
            _bufferSize = requiredSize;
        }

        // Copy data from frame to our buffer
        unsafe
        {
            Buffer.MemoryCopy(frame.Data.ToPointer(), _localBuffer.ToPointer(), _bufferSize, requiredSize);
        }

        // Use our buffer pointer instead of the direct frame pointer
        _buffer.pointer = _localBuffer;
        _buffer.width = frame.Width;
        _buffer.height = frame.Height;
    }
}
