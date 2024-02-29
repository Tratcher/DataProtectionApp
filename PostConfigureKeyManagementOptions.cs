using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.Extensions.Options;
using System.Xml.Linq;

sealed class PostConfigureKeyManagementOptions : IPostConfigureOptions<KeyManagementOptions>
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly string? _keyDirectoryPath;

    public PostConfigureKeyManagementOptions(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _keyDirectoryPath = configuration["ASPNETCORE_READONLY_KEY_DIRECTORY"];// TODO (acasey): better
    }

    void IPostConfigureOptions<KeyManagementOptions>.PostConfigure(string? name, KeyManagementOptions options)
    {
        if (_keyDirectoryPath is null || name != Options.DefaultName)
        {
            return;
        }

        // Code for simulating previous generation of the keyring
        //options.XmlEncryptor = new NullXmlEncryptor();
        //options.XmlRepository = new FileSystemXmlRepository(new DirectoryInfo(_keyDirectoryPath), _loggerFactory);

        // If Data Protection has not been configured, then set it up according to the environment variable
        if (options is { XmlRepository: null, XmlEncryptor: null })
        {
            var keyDirectory = new DirectoryInfo(_keyDirectoryPath);

            options.AutoGenerateKeys = false;
            options.XmlEncryptor = InvalidEncryptor.Instance;
            options.XmlRepository = new FileSystemXmlRepository(keyDirectory, _loggerFactory); // TODO: could wrap this an throw on Add
        }
    }

    private sealed class InvalidEncryptor : IXmlEncryptor
    {
        public static readonly IXmlEncryptor Instance = new InvalidEncryptor();

        private InvalidEncryptor()
        {
        }

        EncryptedXmlInfo IXmlEncryptor.Encrypt(XElement plaintextElement)
        {
            throw new InvalidOperationException("Keys access is set up as read-only, so nothing should be encrypting");
        }
    }
}
