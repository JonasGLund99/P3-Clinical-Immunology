# P3
System for Clinical Immunology at Aalborg University Hospital.

# Running the project:
CD: into the directory 'src'

Open the terminal with ctrl-æ.

To run the project type ```dotnet watch``` and press enter



# Conventions

**Case for class members**
- public members - PascalCase
- private members - camelCase
- backing field - beginning underscore (_camelCase)

**Constructors**
- NO 'this.' in constructors (except 'this.id' in classes inheriting from BaseModel)
- Method parameters in camelCase (including constructors)
- Order of members in class
    - Private constructors
    - Private members
    - Public constructors
    - Public members

**Filenames**
- PascalCase
- *filename*.razor uses *filename*.razor.css

**Access database**
1. Include namespace ```using src.Data```
2. ```DatabaseService.Instance.Database```

