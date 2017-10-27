using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class TaxonomyClass
    {
        int m_class_id = 0;
        TaxonomyClass m_parent_class = null;
        List<TaxonomyClass> m_sub_classes = null;

        public TaxonomyClass(int class_id, TaxonomyClass parent_class)
        {
            m_class_id = class_id;
            m_parent_class = parent_class;
        }

        public int ID
        {
            get { return m_class_id; }
        }

        public TaxonomyClass Parent
        {
            get { return m_parent_class; }
        }

        public void AddSubClass(TaxonomyClass sub_class)
        {
            if (m_sub_classes == null)
                m_sub_classes = new List<TaxonomyClass>();
            m_sub_classes.Add(sub_class);
        }
    }

    public class Taxonomy
    {
        int m_taxonomy_id = 0;
        List<TaxonomyClass> m_root_classes = new List<TaxonomyClass>();
        Dictionary<int, TaxonomyClass> m_all_classes = new Dictionary<int, TaxonomyClass>();

        public Taxonomy(int taxonomy_id)
        {
            m_taxonomy_id = taxonomy_id;
        }

        public int ID
        {
            get { return m_taxonomy_id; }
        }

        public void AddClass(int class_id, int parent_class_id)
        {
            TaxonomyClass parent_class = null;
            if (parent_class_id != 0)
                m_all_classes.TryGetValue(parent_class_id, out parent_class);
            TaxonomyClass sub_class = new TaxonomyClass(class_id, parent_class);
            m_all_classes[class_id] = sub_class;
            if (parent_class != null)
                parent_class.AddSubClass(sub_class);
            else
                m_root_classes.Add(sub_class);
        }

        public TaxonomyClass GetClass(int class_id)
        {
            TaxonomyClass sub_class = null;
            m_all_classes.TryGetValue(class_id, out sub_class);
            return sub_class;
        }
    }

    public class EntityCategorySystem : Singleton<EntityCategorySystem>
    {
        List<Taxonomy> m_taxonomies = new List<Taxonomy>();

        private EntityCategorySystem()
        {
        }

        public override void Destruct()
        {
        }

        public Taxonomy CreateTaxonomy(int taxonomy_id)
        {
            for (int i = 0; i < m_taxonomies.Count; ++i)
            {
                if (m_taxonomies[i].ID == taxonomy_id)
                    return m_taxonomies[i];
            }
            Taxonomy taxonomy = new Taxonomy(taxonomy_id);
            m_taxonomies.Add(taxonomy);
            return taxonomy;
        }

        public bool IsCategory(int sub_class_id, int objective_class_id)
        {
            for (int i = 0; i < m_taxonomies.Count; ++i)
            {
                Taxonomy taxonomy = m_taxonomies[i];
                TaxonomyClass sub_class = taxonomy.GetClass(sub_class_id);
                while (sub_class != null)
                {
                    if (sub_class.ID == objective_class_id)
                        return true;
                    sub_class = sub_class.Parent;
                }
            }
            return false;
        }
    }
}