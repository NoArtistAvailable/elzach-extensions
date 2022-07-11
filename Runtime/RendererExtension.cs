using System.Collections.Generic;
using UnityEngine;

namespace elZach.Common
{
	public static class RendererExtension
	{
		private static MaterialPropertyBlock block = new MaterialPropertyBlock();
		public static void SetPropertyOnBlock(this Renderer renderer, string propertyName, Color value)
		{
			renderer.GetPropertyBlock(block);
			block.SetColor(propertyName, value);
			renderer.SetPropertyBlock(block);
		}
		
		public static void SetPropertyOnBlock(this Renderer renderer, string propertyName, float value)
		{
			renderer.GetPropertyBlock(block);
			block.SetFloat(propertyName, value);
			renderer.SetPropertyBlock(block);
		}
		
		public static void SetPropertyOnBlock(this Renderer renderer, string propertyName, Texture value)
		{
			renderer.GetPropertyBlock(block);
			block.SetTexture(propertyName, value);
			renderer.SetPropertyBlock(block);
		}
		
		public static void SetPropertyOnBlock(this Renderer renderer, string propertyName, Vector4 value)
		{
			renderer.GetPropertyBlock(block);
			block.SetVector(propertyName, value);
			renderer.SetPropertyBlock(block);
		}

		public static Color GetColorFromBlock(this Renderer renderer, string propertyName)
		{
			renderer.GetPropertyBlock(block);
			return block.GetColor(propertyName);
		}
		
		public static float GetFloatFromBlock(this Renderer renderer, string propertyName)
		{
			renderer.GetPropertyBlock(block);
			return block.GetFloat(propertyName);
		}
		
		public static Texture GetTextureFromBlock(this Renderer renderer, string propertyName)
		{
			renderer.GetPropertyBlock(block);
			return block.GetTexture(propertyName);
		}
		
		public static Vector4 GetVectorFromBlock(this Renderer renderer, string propertyName)
		{
			renderer.GetPropertyBlock(block);
			return block.GetVector(propertyName);
		}
	}
}