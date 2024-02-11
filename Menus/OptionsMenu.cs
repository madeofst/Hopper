using Godot;
using System;

namespace Hopper
{
	public class OptionsMenu : Control
	{
		int iMaster;
		int iMusic;
		int iFX;

		private HSlider AllSlider;
		private HSlider MusicSlider;
		private HSlider FXSlider;

		public override void _Ready()
		{
			AllSlider = GetNode<HSlider>("AllControls/MarginContainer/VBoxContainer/All/AllSlider");
			MusicSlider = GetNode<HSlider>("AllControls/MarginContainer/VBoxContainer/Music/MusicSlider");
			FXSlider = GetNode<HSlider>("AllControls/MarginContainer/VBoxContainer/FX/FXSlider");

			iMaster = AudioServer.GetBusIndex("Master");
			iMusic = AudioServer.GetBusIndex("Music");
			iFX = AudioServer.GetBusIndex("FX");

			float MasterVolumeDB = AudioServer.GetBusVolumeDb(iMaster);
			float MusicVolumeDB = AudioServer.GetBusVolumeDb(iMusic);
			float FXVolumeDB = AudioServer.GetBusVolumeDb(iFX);

			double MasterVolumeLinear = CalculateLinear(MasterVolumeDB);
			double MusicVolumeLinear = CalculateLinear(MusicVolumeDB);
			double FXVolumeLinear = CalculateLinear(FXVolumeDB);

			AllSlider.Value = MasterVolumeLinear;
			MusicSlider.Value = MusicVolumeLinear;
			FXSlider.Value = FXVolumeLinear;
		}

		public void MenuPressed()
		{
			QueueFree();
		}

		private float CalculateDB(float linear)
		{
			double result = 10 * Math.Log10(linear * Math.Pow(10, 72/10)) - 72;
			return (float)result;
		}

		private double CalculateLinear(float dB)
		{
			double result = Math.Pow(10, dB/10);
			return result;
		}

		public void OnAllSliderChanged(float value)
		{
			AudioServer.SetBusVolumeDb(iMaster,  CalculateDB(value));
		}

		public void OnMusicSliderChanged(float value)
		{
			AudioServer.SetBusVolumeDb(iMusic,  CalculateDB(value));
		}
		
		public void OnFXSliderChanged(float value)
		{
			AudioServer.SetBusVolumeDb(iFX,  CalculateDB(value));
		}

	}
}


