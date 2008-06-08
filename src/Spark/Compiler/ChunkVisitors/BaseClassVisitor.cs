namespace Spark.Compiler.ChunkVisitors
{
	public class BaseClassVisitor : ChunkVisitor
	{
		public string BaseClass { get; set; }
		public string TModel { get; set; }

		public string BaseClassTypeName
		{
			get
			{
				if (string.IsNullOrEmpty(TModel))
					return BaseClass ?? "Spark.SparkViewBase";

				return string.Format("{0}<{1}>",
									 BaseClass ?? "Spark.SparkViewBase",
									 TModel);
			}
		}

		protected override void Visit(ViewDataModelChunk chunk)
		{
			if (!string.IsNullOrEmpty(TModel) && TModel != chunk.TModel)
			{
				throw new CompilerException(string.Format("Only one viewdata model can be declared. {0} != {1}", TModel,
														  chunk.TModel));
			}
			TModel = chunk.TModel;
		}
	}
}
