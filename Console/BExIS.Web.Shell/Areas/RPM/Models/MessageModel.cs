using System;
using System.Linq;
using System.Collections.Generic;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public class MessageModel
    {
        public bool hasMessage { get; set; }
        public string Message { get; set; }
        public string CssId { get; set; }
        

        public MessageModel()
        {
            this.hasMessage = false;
            this.Message = "";
            this.CssId = "";
        }

        public static MessageModel validateDataStructureName(long Id, string Name, string cssId = "")
        {
            if (Name == "" || string.IsNullOrEmpty(Name))
            {
                return new MessageModel()
                {
                    hasMessage = true,
                    Message = "The Name field is required.",
                    CssId = cssId
                };
            }
            else
            {
                DataStructureManager dataStructureManager = new DataStructureManager();
                List<DataStructure> dataStructures = dataStructureManager.AllTypesDataStructureRepo.Get().ToList();

                foreach (DataStructure ds in dataStructures)
                {
                    if (Id != ds.Id)
                    {
                        if (ds.Name.Trim().ToLower() == Name.Trim().ToLower())
                        {
                            return new MessageModel()
                            {
                                hasMessage = true,
                                Message = "A Datastructure with same Name already exists.",
                                CssId = cssId
                            };
                        }
                    }
                }
            }
            return new MessageModel() { CssId = cssId };
        }

        public static MessageModel validateDataStructureDelete(long Id)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            DataStructure dataStructure = dataStructureManager.AllTypesDataStructureRepo.Get(Id);
            if (dataStructure != null && dataStructure.Id == Id && dataStructure.Datasets.Count == 0)
            {
                return new MessageModel()
                {
                    hasMessage = false,
                    Message = "Are you sure you want to delete the Datastructure " + dataStructure.Name + " (" + Id + ").",
                    CssId = Id.ToString()
                };
            }
            else
            {
                return new MessageModel()
                {
                    hasMessage = true,
                    Message = "Can't delete the Datastructure " + dataStructure.Name + " (" + Id + ").",
                    CssId = Id.ToString()
                };
            }
        }
    }
}

    