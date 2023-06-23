using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UdonSharp.Video;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Audiolink_USVideo_Adapter : UdonSharpBehaviour
{
	
	//Points to the USVPlayer so that Stream mode can be checked
	[Tooltip("Select the USharp Video Player component in the scene")]
    public USharpVideoPlayer videoPlayer;
	
	//Connects to the Audiolink component
	[Tooltip("Select the Audiolink component in the scene")]
	public VRCAudioLink.AudioLink audioLink;
	
	//Connects to the Video audio source
	[Tooltip("Select the audio source that will be used when in stream mode, 'VideoAudioSource' is recommended")]
	public AudioSource videoAudioSource;
	
	//Connects to the Stream audio source
	[Tooltip("Select the audio source that will be used when in stream mode, 'StreamAudioSourceR' is recommended")]
	public AudioSource streamAudioSource;
	
	//Declares a synced variable, used to check the audio settings and set them if necessary.
	[UdonSynced] private bool isStreamMode;

	
	private void Start()
	{
		//Checks to ensure the associated components have all been assigned.
		CheckErrors();		
		
		//Registers to the USharpVideoPlayers events.
		videoPlayer.RegisterCallbackReceiver(this);
	}
	
	//Calls the CheckVidPlayer method any time a video begins loading.
	public void OnUSharpVideoLoadStart()
	{
		CheckVidPlayerMode();
	}
	//Checks if the USVPlayer is in Stream mode, and adjusts the Audiolink audiosource if necessary
	public void CheckVidPlayerMode()
	{
		//Checks if the USVPlayer is in Stream mode
		if (videoPlayer.IsUsingAVProPlayer())
		{
			//Announces the state of stream mode
			log("Video player is in stream mode");
			
			//Sends a network event informing players what state the player is in
			SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,"StreamModeActive");
        }
		else
		{
			//Announces that the player is NOT in stream mode
			log("Video player is NOT in stream mode");
		
			//Sets streamMode bool to false
			isStreamMode = false;
			
			//Sends a network event informing players what state the player is in
			SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,"VideoModeActive");
		}
	}
	public void StreamModeActive()
	{
		//Sets streamMode bool to true
		isStreamMode = true;
		
		//Serializes value
		RequestSerialization();
		
		//Checks if the Audiolink source is set to the appropriate setting and changes it if not set correctly.
		if (audioLink.audioSource == streamAudioSource)
		{
			log($"No action necessary, the player is in video mode and Audiolink is already set to {audioLink.audioSource.name}.");
			return;;
		}
		else
		{
			audioLink.audioSource = streamAudioSource;
			log($"Audio Source set to {audioLink.audioSource.name}.");
		}
	}
	public void VideoModeActive()
	{
		//Sets streamMode bool to false
		isStreamMode = false;
		
		//Serializes value
		RequestSerialization();

		//Checks if the Audiolink source is set to the appropriate setting and changes it if not set correctly.
		if (audioLink.audioSource == videoAudioSource)
		{
		log($"No action necessary, the player is in video mode and Audiolink is already set to {audioLink.audioSource.name}.");
			return;;
		}
		else
		{
			audioLink.audioSource = videoAudioSource;
			log($"Audio Source set to {audioLink.audioSource.name}.");
		}
	}
	public override void OnDeserialization()
	{
		if (isStreamMode == true)
		{
			StreamModeActive();
		}
		else
		{
			VideoModeActive();
		}
	}
	
		private void CheckErrors()
	{
		//Creates an error message if any of the required components have not been assigned.
		if (videoPlayer == null)
		{
			err("No video player has been assigned to the USharpVideoPlayer AudioLink Adapter");
		}
		
		if (audioLink == null)
		{
			err("No audio link component has been assigned to the USharpVideoPlayer AudioLink Adapter");
		}
		
		if (videoAudioSource == null)
		{
			err("No audio source has been assigned to the USharpVideoPlayer AudioLink Adapter for use when in video mode");
		}
		
		if (streamAudioSource == null)
		{
			err("No audio source has been assigned to the USharpVideoPlayer AudioLink Adapter for use when in stream mode");
		}
	}
	
	//Simple method calls for logging
	private void log(string value)
        {
            Debug.Log($"[<color=#ff00c6>{nameof(Audiolink_USVideo_Adapter)}</color>] | {value}");
        }
        private void warn(string value)
        {
            Debug.LogWarning($"[<color=#ff00c6>{nameof(Audiolink_USVideo_Adapter)}</color>] | {value}");
        }
        private void err(string value)
        {
            Debug.LogError($"[<color=#ff00c6>{nameof(Audiolink_USVideo_Adapter)}</color>] | {value}");
        }

}
