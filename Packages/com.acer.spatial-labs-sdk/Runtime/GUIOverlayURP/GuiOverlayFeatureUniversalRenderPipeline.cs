#if ACER_SPATIAL_LABS_RENDER_PIPELINE_UNIVERSAL

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
#endif

namespace Unity.XR.Acer
{
	#if UNITY_6000_0_OR_NEWER
	public class GuiOverlayFeatureUniversalRenderPipeline : ScriptableRendererFeature
	{
		public class TexRefData : ContextItem
		{
			public TextureHandle texture = TextureHandle.nullHandle;

			public override void Reset()
			{
				texture = TextureHandle.nullHandle;
			}
		}

		class UpdateRefPass : ScriptableRenderPass
		{
			class PassData
			{
				public TextureHandle source;
				public TextureHandle destination;

				public Material material;
			}

			static Vector4 scaleBias = new Vector4(1f, 1f, 0f, 0f);

			Material m_renderGraphMaterial;

			public void Setup(Material material)
			{
				m_renderGraphMaterial = material;
			}

			public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
			{
				var texRefExist = frameData.Contains<TexRefData>();
				var texRef = frameData.GetOrCreate<TexRefData>();

				if (!texRefExist)
				{
					var resourceData = frameData.Get<UniversalResourceData>();
					texRef.texture = resourceData.activeColorTexture;
				}

				if (m_renderGraphMaterial != null)
				{
					using (var builder = renderGraph.AddRasterRenderPass<PassData>($"UpdateRefPass_{m_renderGraphMaterial.name}", out var passData))
					{
					passData.source = texRef.texture;

					var descriptor = passData.source.GetDescriptor(renderGraph);

					descriptor.msaaSamples = MSAASamples.None;
					descriptor.name = $"BlitMaterialRefTex_{m_renderGraphMaterial.name}";
					descriptor.clearBuffer = false;

					passData.destination = renderGraph.CreateTexture(descriptor);

					passData.material = m_renderGraphMaterial;

					texRef.texture = passData.destination;

					builder.UseTexture(passData.source);
					builder.SetRenderAttachment(passData.destination, 0);

					builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
					}
				}
			}

			static void ExecutePass(PassData data, RasterGraphContext rgContext)
			{
				Blitter.BlitTexture(rgContext.cmd, data.source, scaleBias, data.material, 0);
			}
		}

		class CopyBackRefPass : ScriptableRenderPass
		{
			public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
			{
				if (!frameData.Contains<TexRefData>()) return;

				var resourceData = frameData.Get<UniversalResourceData>();
				var texRef = frameData.Get<TexRefData>();
				
				renderGraph.AddBlitPass(texRef.texture, resourceData.activeColorTexture, Vector2.one, Vector2.zero, passName: "Blit Back Pass");
			}
		}

		public Material renderGraphMaterial;
		public RenderTexture renderTexture;

		UpdateRefPass m_UpdateRefPass;
		CopyBackRefPass m_CopyBackRefPass;

		public override void Create()
		{		
			#if UNITY_EDITOR
			string[] guids_m = AssetDatabase.FindAssets("GuiOverlayURP_RG t:Material", new[] { "Packages/com.acer.spatial-labs-sdk/Runtime/GUIOverlayURP" });
			string path_m = AssetDatabase.GUIDToAssetPath(guids_m[0]);
			renderGraphMaterial = AssetDatabase.LoadAssetAtPath<Material>(path_m);
			
			string[] guids_t = AssetDatabase.FindAssets("GuiRenderTexture t:RenderTexture", new[] { "Packages/com.acer.spatial-labs-sdk/Runtime/GUIOverlayCommon" });
			string path_t = AssetDatabase.GUIDToAssetPath(guids_t[0]);
			renderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(path_t);	
			#endif
			
			if (renderGraphMaterial != null && renderTexture != null)
			{
				renderGraphMaterial.SetTexture("_texture", renderTexture);
			}
			
			m_UpdateRefPass = new UpdateRefPass();
			m_CopyBackRefPass = new CopyBackRefPass();

			m_UpdateRefPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
			m_CopyBackRefPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{		
			m_UpdateRefPass.Setup(renderGraphMaterial);
			renderer.EnqueuePass(m_UpdateRefPass);
			renderer.EnqueuePass(m_CopyBackRefPass);
		}
	}
	#else
	public sealed class GuiOverlayFeatureUniversalRenderPipeline : ScriptableRendererFeature
	{
		public Material material;
		public RenderTexture renderTexture;

		private GuiOverlay _pass;

		public override void Create()
		{
			_pass = new GuiOverlay
			{
				material = material,
				renderPassEvent = RenderPassEvent.BeforeRenderingTransparents,
			};
			
			#if UNITY_EDITOR
			string[] guids_m = AssetDatabase.FindAssets("GuiOverlayURP t:Material", new[] { "Packages/com.acer.spatial-labs-sdk/Runtime/GUIOverlayURP" });
			string path_m = AssetDatabase.GUIDToAssetPath(guids_m[0]);
			material = AssetDatabase.LoadAssetAtPath<Material>(path_m);

			string[] guids_t = AssetDatabase.FindAssets("GuiRenderTexture t:RenderTexture", new[] { "Packages/com.acer.spatial-labs-sdk/Runtime/GUIOverlayCommon" });
			string path_t = AssetDatabase.GUIDToAssetPath(guids_t[0]);
			renderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(path_t);
			#endif
		}

		#if UNITY_2022
		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (renderingData.cameraData.cameraType != CameraType.Game) return;
			if (renderingData.cameraData.camera.targetTexture == renderTexture) return;
			renderer.EnqueuePass(_pass);
		}
		
		public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
		{
			_pass.ConfigureInput(ScriptableRenderPassInput.Color);
			_pass.SetRenderTarget(renderer.cameraColorTarget);
		}
		#elif UNITY_2021
		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (renderingData.cameraData.cameraType != CameraType.Game) return;
			if (renderingData.cameraData.camera.targetTexture == renderTexture) return;
			_pass.ConfigureInput(ScriptableRenderPassInput.Color);
			_pass.SetRenderTarget(renderer.cameraColorTarget);
			renderer.EnqueuePass(_pass);
		}
		#endif
	}

	public sealed class GuiOverlay : ScriptableRenderPass
	{
		public Material material;

		private RenderTargetIdentifier m_CameraColorTarget;

		public void SetRenderTarget(RenderTargetIdentifier cameraColorTarget)
		{
			m_CameraColorTarget = cameraColorTarget;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			ConfigureTarget(new RenderTargetIdentifier(m_CameraColorTarget, 0, CubemapFace.Unknown, -1));
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var camera = renderingData.cameraData.camera;
			if (camera.cameraType != CameraType.Game) return;

			if (material == null) return;

			var commandBuffer = CommandBufferPool.Get("PostEffect");

			commandBuffer.SetRenderTarget(new RenderTargetIdentifier(m_CameraColorTarget, 0, CubemapFace.Unknown, -1));
			//Blit(commandBuffer, ref renderingData, material, 0);
			commandBuffer.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();

			CommandBufferPool.Release(commandBuffer);
		}
	}
	#endif
}

#endif
