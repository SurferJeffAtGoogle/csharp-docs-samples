using Google.Apis.Storage.v1.Data;
using Google.Storage.V1;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace GoogleCloudSamples
{
    public class QuickStart
    {
        private static readonly string s_projectId = "YOUR-PROJECT-ID";

        private static readonly string s_usage =
                "Usage: \n" +
                "  QuickStart create <new-bucket-name>\n" +
                "  QuickStart list\n" +
                "  QuickStart delete bucket-name\n";

        // [START storage_create_bucket]
        private static void Create(string bucketName)
        {
            var storage = StorageClient.Create();
            if (bucketName == null)
                bucketName = RandomBucketName();
            storage.CreateBucket(s_projectId, new Bucket { Name = bucketName });
            Console.WriteLine($"Created {bucketName}.");
        }
        // [END storage_create_bucket]

        // [START storage_list_buckets]
        private static void List()
        {
            var storage = StorageClient.Create();
            foreach (var bucket in storage.ListBuckets(s_projectId))
            {
                Console.WriteLine(bucket.Name);
            }
        }
        // [END storage_list_buckets]

        // [START storage_delete_bucket]
        private static void Delete(string bucketName)
        {
            var storage = StorageClient.Create();
            storage.DeleteBucket(new Bucket { Name = bucketName });
            Console.WriteLine($"Deleted {bucketName}.");
        }
        // [END storage_delete_bucket]

        public static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(s_usage);
                return -1;
            }
            try
            {
                switch (args[0].ToLower())
                {
                    case "create":
                        Create(args.Length < 2 ? null : args[1]);
                        return 0;

                    case "list":
                        List();
                        return 0;

                    case "delete":
                        if (args.Length < 2)
                        {
                            Console.WriteLine(s_usage);
                            return -1;
                        }
                        Delete(args[1]);
                        return 0;

                    default:
                        Console.WriteLine(s_usage);
                        return -1;
                }
            }
            catch (Google.GoogleApiException e)
            {
                Console.WriteLine(e.Message);
                return e.Error.Code;
            }
        }

        private static string RandomBucketName()
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                string legalChars = "abcdefhijklmnopqrstuvwxyz";
                byte[] randomByte = new byte[1];
                var randomChars = new char[12];
                int nextChar = 0;
                while (nextChar < randomChars.Length)
                {
                    rng.GetBytes(randomByte);
                    if (legalChars.Contains((char)randomByte[0]))
                        randomChars[nextChar++] = (char)randomByte[0];
                }
                return new string(randomChars);
            }
        }
    }
}