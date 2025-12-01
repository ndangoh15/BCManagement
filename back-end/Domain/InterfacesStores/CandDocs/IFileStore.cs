using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.InterfacesStores.CandDocs
{
    public interface IFileStore
    {
        public interface IFileStore
        {
            // --- PATH HELPERS ---
            string BuildBasePath(string session, string exam, string centre);
            string EnsureDir(string path);

            string GetImportedFolder(string session, string exam, string centre);
            string GetSuccessFolder(string session, string exam, string centre);
            string GetErrorFolder(string session, string exam, string centre);

            // --- SAVE FILES ---
            Task<string> SaveSuccessFileAsync(byte[] bytes, string session, string exam, string centre, string fileName);

            // --- MOVE FILES ---
            Task<string> MoveOriginalImportedPdfAsync(byte[] bytes, string session, string exam, string centre, string fileName);

            Task<string> MoveToErrorFolderAsync(string currentPath);
            Task<string> MoveToSuccessFolderAsync(string currentPath);

            // --- UTILITIES ---
            Task<bool> FileExistsAsync(string path);
        }


    }
}
