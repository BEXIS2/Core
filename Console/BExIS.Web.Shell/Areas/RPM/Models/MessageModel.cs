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
            if (Name.Trim() == "" || string.IsNullOrEmpty(Name))
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
            return validateDataStructureDelete(Id, dataStructure);
        }

        public static MessageModel validateDataStructureDelete(long Id , DataStructure dataStructure)
        {
            if (dataStructure != null && dataStructure.Id != 0)
            {
                
                if (dataStructure.Datasets.Count == 0)
                {
                    return new MessageModel()
                    {
                        hasMessage = false,
                        Message = "Are you sure you want to delete the Datastructure " + dataStructure.Name + " (" + dataStructure.Id + ").",
                        CssId = dataStructure.Id.ToString()
                    };
                }
                else
                {
                    try
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Can't delete the Datastructure " + dataStructure.Name + " (" + dataStructure.Id + ").",
                            CssId = dataStructure.Id.ToString()
                        };
                    }
                    catch
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Something is wrong with Datastructure " + Id,
                            CssId = "0"
                        };
                    }
                }
            }
            else
            {
                return new MessageModel()
                {
                    hasMessage = true,
                    Message = "Something is wrong with Datastructure " + Id,
                    CssId = "0"
                };
            }
        }

        public static MessageModel validateDataStructureInUse(long Id)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            DataStructure dataStructure = dataStructureManager.AllTypesDataStructureRepo.Get(Id);
            return validateDataStructureInUse(Id, dataStructure);
        }

        public static MessageModel validateDataStructureInUse(long Id, DataStructure dataStructure)
        {
            if (dataStructure != null && dataStructure.Id != 0)
            {
                if (dataStructure.Datasets.Count > 0)
                {
                    try
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Can't save Datastructure " + dataStructure.Name + " (" + Id + "), it's uesed by a Dataset.",
                            CssId = "inUse"
                        };
                    }
                    catch
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Something is wrong with Datastructure " + Id,
                            CssId = "0"
                        };
                    }
                }
                else
                {
                    return new MessageModel();
                }
            }
            else
            {
                if (Id == 0)
                {
                    return new MessageModel();
                }
                else
                {
                    return new MessageModel()
                    {
                        hasMessage = true,
                        Message = "Can't store Variable for the Datastructure " + Id + ", it's uesed by a Dataset.",
                        CssId = "0"
                    };
                }
            }
            
        }

        public static MessageModel validateAttributeDelete(long Id)
        {
            DataAttribute dataAttribute = new DataContainerManager().DataAttributeRepo.Get(Id);
            return validateAttributeDelete(Id, dataAttribute);
        }

        public static MessageModel validateAttributeDelete(long Id, DataAttribute dataAttribute)
        {
            if (dataAttribute != null && dataAttribute.Id != 0)
            {

                if (dataAttribute.UsagesAsVariable.Count == 0)
                {
                    return new MessageModel()
                    {
                        hasMessage = false,
                        Message = "Are you sure you want to delete the Variable Temlate " + dataAttribute.Name + " (" + dataAttribute.Id + ").",
                        CssId = dataAttribute.Id.ToString()
                    };
                }
                else
                {
                    try
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Can't delete the Variable Temlate " + dataAttribute.Name + " (" + dataAttribute.Id + ").",
                            CssId = dataAttribute.Id.ToString()
                        };
                    }
                    catch
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Something is wrong with Variable Temlate " + Id,
                            CssId = "0"
                        };
                    }
                }
            }
            else
            {
                return new MessageModel()
                {
                    hasMessage = true,
                    Message = "Something is wrong with Variable Temlate " + Id,
                    CssId = "0"
                };
            }
        }

        public static MessageModel validateAttributeName(long Id, string Name, string cssId = "")
        {
            if (Name.Trim() == "" || string.IsNullOrEmpty(Name))
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
                List<DataAttribute> dataAttributes = new DataContainerManager().DataAttributeRepo.Get().ToList();
  
                foreach (DataAttribute da in dataAttributes)
                {
                    if (Id != da.Id)
                    {
                        if (da.Name.Trim().ToLower() == Name.Trim().ToLower())
                        {
                            return new MessageModel()
                            {
                                hasMessage = true,
                                Message = "A Variable Template with same Name already exists.",
                                CssId = cssId
                            };
                        }
                    }
                }
            }
            return new MessageModel() { CssId = cssId };
        }

        public static MessageModel validateAttributeInUse(long Id)
        {
            DataContainerManager dataAttributeManager = new DataContainerManager();
            DataAttribute dataAttribute = dataAttributeManager.DataAttributeRepo.Get(Id);
            return validateAttributeInUse(Id, dataAttribute);
        }

        public static MessageModel validateAttributeInUse(long Id, DataAttribute dataAttribute)
        {
            if (dataAttribute != null && dataAttribute.Id != 0)
            {
                if (dataAttribute.UsagesAsVariable.Count > 0)
                {
                    try
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Can't save Variable Template " + dataAttribute.Name + " (" + Id + "), it's uesed by a Data Structure.",
                            CssId = "inUse"
                        };
                    }
                    catch
                    {
                        return new MessageModel()
                        {
                            hasMessage = true,
                            Message = "Something is wrong with Variable Template " + Id,
                            CssId = "0"
                        };
                    }
                }
                else
                {
                    return new MessageModel();
                }
            }
            else
            {
                if (Id == 0)
                {
                    return new MessageModel();
                }
                else
                {
                    return new MessageModel()
                    {
                        hasMessage = true,
                        Message = "Can't save Variable Template " + Id + ", it's uesed by a Data Structure.",
                        CssId = "0"
                    };
                }
            }

        }
    }
}

    