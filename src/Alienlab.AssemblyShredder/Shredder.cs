namespace Alienlab.AssemblyShredder
{
  using System;
  using System.IO;

  using Mono.Cecil;
  using Mono.Cecil.Cil;
  using Mono.Cecil.Rocks;

  public static class Shredder
  {
    /// <summary>
    /// Removes the code of all the methods, properties, events and constructors in the assembly, remaining only signatures.
    /// </summary>
    /// <param name="assemblyFilePath">The file path of the assembly that needs removing instructions.</param>
    /// <param name="outputFilePath">The optional output file name, if null or empty overwrites the given one.</param>
    public static void RemoveCode(string assemblyFilePath, string outputFilePath = null)
    {
      if (!File.Exists(assemblyFilePath))
      {
        return;
      }

      var assembly = AssemblyDefinition.ReadAssembly(assemblyFilePath);
      if (assembly == null)
      {
        throw new InvalidOperationException("Cannot read the assembly: " + assemblyFilePath);

      }

      var modules = assembly.Modules;
      if (modules == null)
      {
        throw new InvalidOperationException("Cannot find modules in the assembly: " + assemblyFilePath);
      }

      foreach (var module in modules)
      {
        if (module == null)
        {
          continue;
        }

        var types = module.Types;
        if (types == null)
        {
          continue;
        }

        foreach (var type in types)
        {
          if (type == null)
          {
            continue;
          }

          if (type.Methods != null)
          {
            foreach (var method in type.Methods)
            {
              if (method == null)
              {
                continue;
              }

              NopRet(method.Body);
            }
          }

          if (type.Properties != null)
          {
            foreach (var property in type.Properties)
            {
              if (property == null)
              {
                continue;
              }

              if (property.SetMethod != null)
              {
                NopRet(property.SetMethod.Body);
              }

              if (property.GetMethod != null)
              {
                NopRet(property.GetMethod.Body);
              }
            }
          }

          if (type.Events != null)
          {
            foreach (var property in type.Events)
            {
              if (property == null)
              {
                continue;
              }

              if (property.AddMethod != null)
              {
                NopRet(property.AddMethod.Body);
              }

              if (property.RemoveMethod != null)
              {
                NopRet(property.RemoveMethod.Body);
              }
            }
          }

          foreach (var constructor in type.GetConstructors())
          {
            if (constructor == null)
            {
              continue;
            }

            NopRet(constructor.Body);
          }
        }

        module.Write(assemblyFilePath);
      }
    }

    private static void NopRet(Mono.Cecil.Cil.MethodBody methodBody)
    {
      if (methodBody == null)
      {
        return;
      }

      var ilProcessor = methodBody.GetILProcessor();
      if (ilProcessor == null)
      {
        return;
      }

      foreach (var instruction in methodBody.Instructions.ToArray())
      {
        ilProcessor.Remove(instruction);
      }

      ilProcessor.Append(ilProcessor.Create(OpCodes.Nop));
      ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
    }
  }
}
