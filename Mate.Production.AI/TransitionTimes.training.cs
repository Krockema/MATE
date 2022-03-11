﻿﻿// This file was auto-generated by ML.NET Model Builder. 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.ML.Trainers;
using Microsoft.ML;

namespace Mate_Production_AI
{
    public partial class TransitionTimes
    {
        public static ITransformer RetrainPipeline(MLContext context, IDataView trainData)
        {
            var pipeline = BuildPipeline(context);
            var model = pipeline.Fit(trainData);

            return model;
        }

        /// <summary>
        /// build the pipeline that is used from model builder. Use this function to retrain model.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <returns></returns>
        public static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
        {
            // Data process configuration with pipeline data transformations
            var pipeline = mlContext.Transforms.ReplaceMissingValues(new []{new InputOutputColumnPair(@"TotalProcessingDuration", @"TotalProcessingDuration"),new InputOutputColumnPair(@"LongestPathProcessingDuration", @"LongestPathProcessingDuration"),new InputOutputColumnPair(@"TimeToRelease", @"TimeToRelease")})      
                                    .Append(mlContext.Transforms.Concatenate(@"Features", new []{@"TotalProcessingDuration",@"LongestPathProcessingDuration",@"TimeToRelease"}))      
                                    .Append(mlContext.Transforms.NormalizeMinMax(@"Features", @"Features"))      
                                    .Append(mlContext.Regression.Trainers.FastTreeTweedie(new FastTreeTweedieTrainer.Options(){NumberOfLeaves=24,MinimumExampleCountPerLeaf=22,NumberOfTrees=46,MaximumBinCountPerFeature=152,LearningRate=0.000600181099819822F,FeatureFraction=0.700569014461289F,LabelColumnName=@"TimeBeforeFinish",FeatureColumnName=@"Features"}));

            return pipeline;
        }
    }
}
