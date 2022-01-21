using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using System.IO;
using System;
using Amazon.S3.Util;
using System.Collections.Generic;
using Amazon.CognitoIdentity;
using Amazon;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

public class AWSManager : MonoBehaviour
{

    private static AWSManager _instance;

    public static AWSManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("AWS Manager is null");
            }

            return _instance;
        }
    }

    public string S3Region = RegionEndpoint.USEast2.SystemName;
    private RegionEndpoint _S3Region
    {
        get 
        { 
            return RegionEndpoint.GetBySystemName(S3Region); 
        }
    }

    private AmazonS3Client _s3Client;
    public AmazonS3Client S3Client
    {
        get
        {
            if (_s3Client == null)
            {
                _s3Client = new AmazonS3Client(new CognitoAWSCredentials(
                    "us-east-2:a075bf1c-86eb-40a7-ac39-9fbad230dcba",
                    RegionEndpoint.USEast2
                ), _S3Region);
            }

            return _s3Client;
        }
    }
    private void Awake() 
    {
        _instance = this;

        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        S3Client.ListBucketsAsync(new ListBucketsRequest(), (responseObject) =>
        {
            if (responseObject.Exception == null)
            {
                responseObject.Response.Buckets.ForEach((s3b) =>
                {
                    Debug.Log("Bucket Name: " + s3b.BucketName);
                });
            }
            else
            {
                Debug.LogError("AWS Error: " + responseObject.Exception);
            }
        });
    }

    public void UploadToS3(string path, string caseID)
    {
        FileStream stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

        PostObjectRequest request = new PostObjectRequest()
        {
            Bucket = "servicegarfgamedev",
            Key = "case#" + caseID,
            InputStream = stream,
            CannedACL = S3CannedACL.Private,
            Region = _S3Region
        };

        S3Client.PostObjectAsync(request, (responseObj) =>
        {
            if (responseObj.Exception == null)
            {
                Debug.Log("Successfully postedd to bucket");
                SceneManager.LoadScene(0);
            }
            else
            {
                Debug.Log("Exception occured during uploading: " + responseObj.Exception);
            }
        });
    }

    public void GetList(string caseNumber, Action onComplete = null)
    {
        string target = "case#" + caseNumber;

        Debug.Log("Running the GetList method in AWSManager");

        var request = new ListObjectsRequest()
        {
            BucketName = "servicegarfgamedev"
        };

        S3Client.ListObjectsAsync(request, (responseObject) =>
        {
            if (responseObject.Exception == null)
            {
                bool caseFound = responseObject.Response.S3Objects.Any(obj => obj.Key == target);

                if (caseFound == true)
                {
                    Debug.Log("Found case file!");
                    S3Client.GetObjectAsync("servicegarfgamedev", target, (responseObj) =>
                    {
                        // read the data and apply it to a case (object) to be used
                        // check if response stream is null
                        if (responseObj.Response.ResponseStream != null)
                        {
                            // byte array to store data from file
                            byte[] data = null;

                            // use streamreader to read response data
                            using (StreamReader reader = new StreamReader(responseObj.Response.ResponseStream))
                            {
                                // access a memory stream
                                using (MemoryStream memory = new MemoryStream())
                                {
                                    // populate date byte array with memory stream data
                                    var buffer = new byte[512];
                                    var bytesRead = default(int);

                                    while ((bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        memory.Write(buffer, 0, bytesRead);
                                    }
                                    data = memory.ToArray();
                                }
                            }
                            using (MemoryStream memory = new MemoryStream(data))
                            {
                                BinaryFormatter bf = new BinaryFormatter();
                                Case downloadedCase = (Case)bf.Deserialize(memory);
                                Debug.Log("Downloaded case name: " + downloadedCase.name);
                                UIManager.Instance.activeCase = downloadedCase;
                                if (onComplete != null)
                                {
                                    onComplete();
                                }
                            }
                        }



                        
                        
                    });
                }
                else
                {
                    Debug.Log("Did not find case");
                }
            }
            else
            {
                Debug.Log("Error getting list of items from s3 " + responseObject.Exception);
            }
        });
    }
    
}
