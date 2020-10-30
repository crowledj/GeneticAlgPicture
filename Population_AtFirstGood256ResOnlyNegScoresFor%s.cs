using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Media;

namespace MakeLand
{
    public class Population
    {
        public int generation = 0;
        public int bestScore = 0;
        public int bestIndex = 0;
        public int numInPop = 0;
        public int[] listOfLiving;
        public int countOfLiving;

        public Phenotype[] maps = null;

        public Population(int numInPopZ, Random r)
        { 
            numInPop = numInPopZ;
            maps = new Phenotype[numInPop];
            for (int i = 0; i < numInPop; i++)
            {
                Genotype g = new Genotype(r);
                Phenotype p = new Phenotype(g,0);
                p.createPheno();
                p.setScore();
                maps[i] = p; 
            }
        }

        /// <summary>
        /// Returns the index of the best individual and updates bestScore
        /// </summary>
        /// <returns></returns>
        public int findBest()
        {
            Phenotype p = maps[0];
            bestScore = p.score;
            bestIndex = 0;
            for (int i = 1; i < numInPop; i++)
            {
                p = maps[i];
                if (p.score > bestScore)
                {
                    bestIndex = i;
                    bestScore = p.score;

                    //adding this assignment as I wish to find best k individuals to act as parents for crossover -> creat offspring
                    //maps[i].score = -1;

                }
            }
            return bestIndex;
        }


        /// <summary>
        /// Finds the worst individual thats actually alive
        /// </summary>
        /// <returns></returns>
        public int findWorstAlive()
        {
            bool first = true;
            int worstScore = 0;
            int worstIndex = 0;

            for (int i = 1; i < numInPop; i++)
            {
                Phenotype p = maps[i];
                if (p.alive && first)
                {
                    first = false;
                    worstScore = p.score;
                    worstIndex = i;
                    continue;
                }
                    
                if (p.alive && p.score < worstScore)
                {
                    worstScore = p.score;
                    worstIndex = i;
                }
            }
            return worstIndex;
        }

        /// <summary>
        /// Just a standard getter
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Phenotype getPhenotype(int i)
        {
            return maps[i];
        }

        /// <summary>
        /// Unsets the newborn flag for the entire population
        /// </summary>
        public void unsetNewborn()
        {
            for (int i = 1; i < numInPop; i++)
            {
                getPhenotype(i).newborn = false;
            }
        }

        /// <summary>
        /// Kills the weakest
        /// </summary>
        /// <param name="n"></param>
        public void killThisMany(int n)
        {

            for (int i = 0; i <n; i++)
            {
                int k = findWorstAlive();
                getPhenotype(k).alive = false;
            }
        }



        /// <summary>
        /// Search for dead individuals - replace them with living newborn ones
        /// </summary>
        public void breedPopulation(Random r)
        {
            listOfLiving = new int[Params.populationCnt];
            countOfLiving=0;
            for (int i = 0; i < Params.populationCnt; i++)
            {
                if (getPhenotype(i).alive && (!getPhenotype(i).newborn))
                {
                    listOfLiving[i] = i;
                    countOfLiving++;
                }
            }


            for (int i = 0; i < Params.populationCnt; i++)
            {
                if (!getPhenotype(i).alive)
                {
                    int mum =   r.Next(0, countOfLiving); //findBest();
                    int dad =   r.Next(0, countOfLiving); //findBest();
                    mum = listOfLiving[mum];
                    dad = listOfLiving[dad];
                    Phenotype mumP = getPhenotype(mum);
                    Phenotype dadP = getPhenotype(dad);
                    Genotype ggg = makeGenome(mumP.genotype,dadP.genotype);
                    if (Params.mutationPercent > r.Next(0,100)) mutate(ggg, r);
                    //checkDuplicateGenes(ggg);
                    maps[i] = new Phenotype(ggg, G.pop.generation);

                }
            }
        }


        public bool checkDuplicateGenes(Genotype ggg)
        {
            bool retv = false;
            for (int i = 0; i < Params.genotypeSize; i++)
                for (int k = i+1; k < Params.genotypeSize; k++)
                {
                    if(ggg.genes[i].equal(ggg.genes[k]))
                      {
                        G.dupGeneCount++;
                        ggg.genes[i] = new Gene(G.rnd);
                        retv = true;
                      }
                }
            return retv;
        }

        public void mutate(Genotype g, Random r)
        {

            // generate a random no. in [1,6] to choose which mutation strategy to emplpy each time:
            int toggleMutate = (int) (r.NextDouble() * 6);
            int num_mutates = toggleMutate * 5;
            //String togStr = toggleMutate.ToString();
            //Console.WriteLine("toggleMutate = " + togStr);

            switch (toggleMutate) { 

                case 1:

                    //mutate a couple (3 - fixed for now) random indices's repeatX  along the genotype
                    for (int k=0; k < num_mutates ; k++) {

                    int mutate_index = (int)r.NextDouble() * Params.genotypeSize;

                    if (G.rnd.NextDouble() < 0.5)
                        g.genes[mutate_index].repeatX += (int)r.NextDouble() * 5; //int(1 / r);
                    else
                        g.genes[mutate_index].repeatX -= (int)r.NextDouble() * 5;

                    }
                    break;

                case 2:

                    //mutate a couple (3 - fixed for now) random indices' x value along the genotype
                    for (int k = 0; k < num_mutates; k++)
                    {

                        int mutate_index = (int)r.NextDouble() * Params.genotypeSize;

                        if (G.rnd.NextDouble() < 0.5)
                            g.genes[mutate_index].x -= (int)r.NextDouble() * 5; //int(1 / r);
                        else
                            g.genes[mutate_index].x += (int)r.NextDouble() * 5;


                    }
                    break;

                case 3:

                    //mutate a couple (3 - fixed for now) random indices's repeatY value along the genotype
                    for (int k = 0; k < num_mutates; k++)
                    {

                        int mutate_index = (int)r.NextDouble() * Params.genotypeSize;

                        if (G.rnd.NextDouble() < 0.5)
                            g.genes[mutate_index].repeatY += (int)r.NextDouble() * 5; //int(1 / r);
                        else
                            g.genes[mutate_index].repeatY -= (int)r.NextDouble() * 5;

                    }
                    break;

                case 4:

                    //mutate a couple (3 - fixed for now) random indices' y value along the genotype
                    for (int k = 0; k < num_mutates; k++)
                    {

                        int mutate_index = (int)r.NextDouble() * Params.genotypeSize;

                        if (G.rnd.NextDouble() < 0.5)
                            g.genes[mutate_index].y += (int)r.NextDouble() * 5; //int(1 / r);
                        else
                            g.genes[mutate_index].y -= (int)r.NextDouble() * 5;
                    }
                    break;

                case 5:

                    //replace gene with a new randomly generated one:
                    g = new Genotype(r);
                    break;

               case 6:
               
                   //grab another genotype and swap with g :
               
                   Genotype swap_genotype = getPhenotype((int)r.NextDouble() * numInPop).genotype;
                   Genotype temp = new Genotype(r);
               
                   //swap them using the temp as intermediary:
                   temp = g;
                   g = swap_genotype;
                   swap_genotype = temp;
                   break;

                default:
                    break;
            }

            G.mutationCount++;
            

        }

        /// <summary>
        /// create a new geneome from mum and dad
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        public Genotype makeGenome(Genotype g1, Genotype g2)
        {
            Genotype retv = new Genotype();
            for (int i = 0; i < Params.genotypeSize; i++)
            {
                if (G.rnd.NextDouble()<0.5)
                {
                    retv.genes[i] = new Gene(g1.genes[i]);
                }
                else
                {
                    retv.genes[i] = new Gene(g2.genes[i]);
                }
            }
            return retv;
        }

        public void checkDuplicateGenotypes()
        {
            for (int i = 0; i < Params.populationCnt; i++)
            {
                Genotype g = getPhenotype(i).genotype;
                if (checkDuplicateGenes(g)) continue;
                for (int k = i + 1; k < Params.populationCnt; k++)
                {
                    Genotype kk = getPhenotype(k).genotype;
                    if (kk.equal(g))
                    {
                        mutate(g, G.rnd);
                        G.dupGeneomeCount++;
                    }
                }
            }
        }


        /// <summary>
        /// what it sounds like
        /// </summary>
        public void do1Generation()
        {
            int genNum = G.pop.generation++;

            //Console.WriteLine("generation no. = " + genNum.ToString() + " best Score = " + bestScore.ToString());


            unsetNewborn();
            killThisMany(Params.populationCnt / 2);
            breedPopulation(G.rnd);
            //if (Params.checkDuplicateGenomes != -1 && G.pop.generation % Params.checkDuplicateGenomes == 0) checkDuplicateGenotypes(); 
            Application.DoEvents();
        }

    }

    public class Genotype
    {
        public Gene[] genes = new Gene[Params.genotypeSize];

        public Genotype(Random r)
        {
            for (int i = 0; i < Params.genotypeSize; i++)
                genes[i] = new Gene(r);
        }

        public Genotype()
        {
            for (int i = 0; i < Params.genotypeSize; i++)
                genes[i] = new Gene();
        }

        public bool equal(Genotype gg)
        {
            for (int i = 0; i < Params.genotypeSize; i++)
            {
                if (!(gg.genes[i].equal(genes[i]))) return false;
            }
            return true;
        }
    }



    public class Gene
    {
        public int terrain=0;
        public int x=0;
        public int y=0;
        public int repeatY = 0;
        public int repeatX = 0;

        public Gene()
        {

        }

        public Gene(int ter, int xx, int yy, int rptX, int rptY)
        {
            terrain = ter;
            x = xx;
            y = yy;
            repeatX = rptX;
            repeatY = rptY;
        }

        /// <summary>
        /// New Random Gene
        /// </summary>
        /// <param name="r"></param>
        public Gene(Random r)
        {
            terrain = r.Next(0,3);
            x = r.Next(0, Params.dimX);
            y = r.Next(0, Params.dimY);
            repeatX = r.Next(0, Params.maxRepeat);
            repeatY = r.Next(0, Params.maxRepeat);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="gg"></param>
        public Gene(Gene gg) // copy constructor
        {
            terrain = gg.terrain;
            x = gg.x;
            y = gg.y;
            repeatX = gg.repeatX;
            repeatY = gg.repeatY;
        }

        public bool equal(Gene g)
        {
            if (g.x != x) return false;
            if (g.y != y) return false;
            if (g.repeatX != repeatX) return false;
            if (g.repeatY != repeatY) return false;
            if (g.terrain != terrain) return false;
            return true;
        }

    }

    public class Phenotype
    {
        public Genotype genotype=null; // reference class - this is a pointer not a copy
        int[,] pheno = null;
        Bitmap bitm = null;
        public int score = 0;
        public bool alive = true;
        public bool newborn = true;
        public int gen = 0;

        /// <summary>
        /// helper funcion for the setScore() routine , this prevents the function becoming too 'clunky' and just handles all the border/edge cases separately
        /// </summary>
        public void scoringBordersHelperFunc(int i,int j)
        {
            //create a summation for these'conecting' pixels which can be used in the checks ahead...
            int phenoTerrainSum = 0; // pheno[x - 1, y] + pheno[x + 1, y] + pheno[x, y + 1] + pheno[x, y - 1];
            if (i == Params.dimX - 1)
            {
                //cater for corner cases first - (i,j) = (Params.dimX - 1,Params.dimX - 1)
                if (j == Params.dimX - 1)
                {
                    if (pheno[i, j] == 1 && ((pheno[i - 1, j] == 1 && pheno[i, j - 1] != 1)
                         || (pheno[i, j - 1] == 1 && pheno[i - 1, j] != 1)
                         || (pheno[i - 1, j] == 1 && pheno[i, j - 1] == 1)))
                    {
                        //Console.WriteLine(" 3rd if block - land connected to mostlj other land - score up part of setScore func. ");
                        score += 5;
                    }

                    if (pheno[i, j] == 0 && (pheno[i - 1, j] == 1 && pheno[i, j - 1] != 1)
                                || (pheno[i, j - 1] == 1 && pheno[i - 1, j] != 1)
                                || (pheno[i - 1, j] == 1 && pheno[i, j - 1] == 1))
                    {
                        //Console.WriteLine(" 4th if block - SetScore func....");
                        score -= 3;
                    }
                }

                else if (j == 0)
                {
                    if (pheno[i, j] == 1 && ((pheno[i - 1, j] == 1 && pheno[i, j + 1] == 1 )
                                || (pheno[i, j + 1] == 1 && pheno[i - 1, j] != 1)
                                || (pheno[i - 1, j] == 1 && pheno[i, j + 1] == 1)))
                                               {
                        //Console.WriteLine(" 3rd if block - land connected to mostlj other land - score up part of setScore func. ");
                        score += 5;
                    }

                    if (pheno[i, j] == 1 && (phenoTerrainSum > 6 | phenoTerrainSum < 2))
                    {
                        //Console.WriteLine("2nd if block :  in land and one neighbour is water - score up part of setScore func. ");
                        score -= 2;
                    }


                    if (pheno[i, j] == 0 && (pheno[i - 1, j] == 1 && pheno[i, j + 1] == 1)
                                || (pheno[i - 1, j] == 1  && pheno[i, j + 1] != 1)
                                || (pheno[i, j + 1] == 1 && pheno[i - 1, j] != 1))
                    {
                        //Console.WriteLine(" 4th if block - SetScore func....");
                        score -= 3;
                    }
                }

                else {
                    //Now can update this 'conecting' pixel sum  safely and correctly...
                    phenoTerrainSum = pheno[i - 1, j] + pheno[i, j + 1] + pheno[i, j - 1];

                    if (pheno[i, j] == 1 && ((pheno[i - 1, j] == 1 && pheno[i, j + 1] == 1 && pheno[i, j - 1] != 1)
                        || (pheno[i - 1, j] == 1 && pheno[i, j - 1] == 1 && pheno[i, j + 1] != 1)
                        || (pheno[i, j - 1] == 1 && pheno[i, j + 1] == 1 && pheno[i - 1, j] != 1)
                        || (pheno[i - 1, j] == 1 && pheno[i, j - 1] == 1 && pheno[i, j + 1] == 1)
                        || (pheno[i - 1, j] == 1 && pheno[i, j - 1] == 1 && pheno[i, j + 1] == 1)))
                    {
                        //Console.WriteLine(" 3rd if block - land connected to mostlj other land - score up part of setScore func. ");
                        score += 5;
                    }

                    if (pheno[i, j] == 1 && (phenoTerrainSum > 6 | phenoTerrainSum < 2))
                    {
                        //Console.WriteLine("2nd if block :  in land and one neighbour is water - score up part of setScore func. ");
                        score -= 2;
                    }


                    if (pheno[i, j] == 0 && (pheno[i - 1, j] == 1 && pheno[i, j + 1] == 1 && pheno[i, j - 1] != 1)
                                || (pheno[i - 1, j] == 1 && pheno[i, j - 1] == 1 && pheno[i, j + 1] != 1)
                                || (pheno[i, j - 1] == 1 && pheno[i, j + 1] == 1 && pheno[i - 1, j] != 1)
                                || (pheno[i - 1, j] == 1 && pheno[i, j - 1] != 1 && pheno[i, j + 1] == 1)
                                || (pheno[i - 1, j] == 1 && pheno[i, j - 1] == 1 && pheno[i, j + 1] == 1))
                    {
                        //Console.WriteLine(" 4th if block - SetScore func....");
                        score -= 3;
                    }
                }//end else
            }


            if (j == Params.dimX - 1)
            {

                if (i == Params.dimX - 1)
                {
                   //land connected to mostly other land (say at least 75 % of horizonal & vert. cells) - score up
                    if (pheno[i, j] == 1 && ((pheno[i - 1, j] == 1 && pheno[i, j - 1] != 1)
                                    || (pheno[i, j - 1] == 1 && pheno[i - 1, j] != 1)
                                    || (pheno[i - 1, j] == 1 && pheno[i, j - 1] == 1)))
                    {
                        //Console.WriteLine(" 3rd if block - land connected to mostlj other land - score up part of setScore func. ");
                        score += 5;
                    }

                    if ((pheno[i, j] == 0 || pheno[i, j] == 2) && (pheno[i - 1, j] == 1 && pheno[i, j - 1] != 1)
                                         || (pheno[i, j - 1] == 1 && pheno[i - 1, j] != 1)
                                         || (pheno[i - 1, j] == 1 && pheno[i, j - 1] == 1))
                    {
                        //Console.WriteLine(" 4th if block - SetScore func....");
                        score -= 3;
                    }
                }

                else if (i == 0)
                {
                    //land connected to mostly other land (say at least 75 % of horizonal & vert. cells) - score up
                    if (pheno[i, j] == 1 && ((pheno[i + 1, j] == 1 && pheno[i, j - 1] != 1)
                                    || (pheno[i, j - 1] == 1 && pheno[i + 1, j] != 1)
                                    || (pheno[i, j - 1] == 1 && pheno[i + 1, j] == 1)))
                    {
                        //Console.WriteLine(" 3rd if block - land connected to mostlj other land - score up part of setScore func. ");
                        score += 5;
                    }

                    if (pheno[i, j] == 1 && (phenoTerrainSum > 6 | phenoTerrainSum < 2))
                    {
                        //Console.WriteLine("2nd if block :  in land and one neighbour is water - score up part of setScore func. ");
                        score -= 2;
                    }


                    if ((pheno[i, j] == 0 || pheno[i, j] == 2) && (pheno[i + 1, j] == 1 && pheno[i, j - 1] != 1)
                                         || (pheno[i, j - 1] == 1 && pheno[i + 1, j] != 1)
                                         || (pheno[i, j - 1] == 1 && pheno[i + 1, j] == 1))
                    {
                        //Console.WriteLine(" 4th if block - SetScore func....");
                        score -= 3;
                    }
                }

                else
                {

                    //Now can update this 'conecting' pixel sum  safely and correctly...
                    phenoTerrainSum = pheno[i - 1, j] + pheno[i, j - 1] + pheno[i + 1, j];

                    //land connected to mostly other land (say at least 75 % of horizonal & vert. cells) - score up
                    if (pheno[i, j] == 1 && ((pheno[i - 1, j] == 1 && pheno[i + 1, j] == 1 && pheno[i, j - 1] != 1)
                                    || (pheno[i - 1, j] == 1 && pheno[i + 1, j] == 1 && pheno[i, j - 1] != 1)
                                    || (pheno[i, j - 1] == 1 && pheno[i + 1, j] == 1 && pheno[i - 1, j] != 1)
                                    || (pheno[i - 1, j] == 1 && pheno[i, j - 1] == 1 && pheno[i + 1, j] != 1)
                                    || (pheno[i - 1, j] == 1 && pheno[i, j - 1] == 1 && pheno[i + 1, j] == 1)))
                    {
                        //Console.WriteLine(" 3rd if block - land connected to mostlj other land - score up part of setScore func. ");
                        score += 5;
                    }

                    if (pheno[i, j] == 1 && (phenoTerrainSum > 6 | phenoTerrainSum < 2))
                    {
                        //Console.WriteLine("2nd if block :  in land and one neighbour is water - score up part of setScore func. ");
                        score -= 2;
                    }


                    if ((pheno[i, j] == 0 || pheno[i, j] == 2) && (pheno[i - 1, j] == 1 && pheno[i + 1, j] == 1 && pheno[i, j - 1] != 1)
                                         || (pheno[i - 1, j] == 1 && pheno[i + 1, j] == 1 && pheno[i, j - 1] != 1)
                                         || (pheno[i, j - 1] == 1 && pheno[i + 1, j] == 1 && pheno[i - 1, j] != 1)
                                         || (pheno[i - 1, j] == 1 && pheno[i, j - 1] == 1 && pheno[i + 1, j] != 1)
                                         || (pheno[i - 1, j] == 1 && pheno[i, j - 1] == 1 && pheno[i + 1, j] == 1))
                    {
                        //Console.WriteLine(" 4th if block - SetScore func....");
                        score -= 3;
                    }
                }
            }


          //if (i == 0)
          //{
          //
          //    //Now can update this 'conecting' pixel sum  safely and correctly...
          //    phenoTerrainSum =  pheno[i, j + 1] + pheno[i, j - 1] + pheno[i + 1, j];
          //
          //    //land connected to mostly other land (say at least 75 % of horizonal & vert. cells) - score up
          //    if (pheno[i, j] == 1 && (( pheno[i + 1, j] == 1 && pheno[i, j + 1] == 1 && pheno[i, j - 1] != 1)
          //                    || (pheno[i + 1, j] == 1 && pheno[i, j - 1] == 1 && pheno[i, j + 1] != 1)
          //                    || (pheno[i, j - 1] == 1 && pheno[i + 1, j] == 1 && pheno[i, j + 1] != 1 )
          //                    || (pheno[i, j - 1] == 1 && pheno[i, j + 1] == 1 && pheno[i + 1, j] != 1)
          //                    || (pheno[i, j - 1] == 1 && pheno[i, j + 1] == 1 && pheno[i + 1, j] == 1)))
          //    {
          //        //Console.WriteLine(" 3rd if block - land connected to mostlj other land - score up part of setScore func. ");
          //        score += 5;
          //    }
          //
          //    if (pheno[i, j] == 1 && (phenoTerrainSum > 6 | phenoTerrainSum < 2))
          //    {
          //        //Console.WriteLine("2nd if block :  in land and one neighbour is water - score up part of setScore func. ");
          //        score -= 2;
          //    }
          //
          //
          //    if ((pheno[i, j] == 0 || pheno[i, j] == 2) && (pheno[i + 1, j] == 1 && pheno[i, j + 1] == 1 && pheno[i, j - 1] != 1)
          //                         || ( pheno[i + 1, j] == 1 && pheno[i, j - 1] == 1 && pheno[i, j + 1] != 1)
          //                         || (pheno[i, j - 1] != 1 && pheno[i + 1, j] == 1 && pheno[i, j + 1] == 1 )
          //                         || (pheno[i, j - 1] == 1 && pheno[i, j + 1] == 1 && pheno[i + 1, j] != 1)
          //                         || (pheno[i, j - 1] == 1 && pheno[i, j + 1] == 1 && pheno[i + 1, j] == 1)){
          //        //Console.WriteLine(" 4th if block - SetScore func....");
          //        score -= 3; }
          //}
          //
          //
          //if (j == 0)
          //{
          //
          //
          //    //Now can update this 'conecting' pixel sum  safely and correctly...
          //    phenoTerrainSum = pheno[i - 1, j] + pheno[i, j + 1]  + pheno[i + 1, j];
          //
          //    //land connected to mostly other land (say at least 75 % of horizonal & vert. cells) - score up
          //    if (pheno[i, j] == 1 && ((pheno[i - 1, j] == 1 && pheno[i + 1, j] == 1 && pheno[i, j + 1] == 1 )
          //                    || (pheno[i - 1, j] == 1 && pheno[i + 1, j] == 1 && pheno[i, j + 1] != 1)
          //                    || (pheno[i + 1, j] == 1 && pheno[i, j + 1] == 1 && pheno[i - 1, j] != 1)
          //                    || (pheno[i - 1, j] == 1 && pheno[i, j + 1] == 1 && pheno[i + 1, j] != 1))){
          //        //Console.WriteLine(" 3rd if block - land connected to mostlj other land - score up part of setScore func. ");
          //        score += 5;
          //    }
          //
          //    if (pheno[i, j] == 1 && (phenoTerrainSum > 6 | phenoTerrainSum < 2)) {
          //        //Console.WriteLine("2nd if block :  in land and one neighbour is water - score up part of setScore func. ");
          //        score -= 2;}
          //
          //
          //    if ((pheno[i, j] == 0 || pheno[i, j] == 2) && (pheno[i - 1, j] == 1 && pheno[i + 1, j] == 1 && pheno[i, j + 1] == 1)
          //                         || (pheno[i - 1, j] == 1 && pheno[i + 1, j] == 1 &&  pheno[i, j + 1] != 1)
          //                         || (pheno[i + 1, j] == 1 && pheno[i, j + 1] == 1 && pheno[i - 1, j] != 1)
          //                         || (pheno[i - 1, j] == 1 && pheno[i, j + 1] == 1 && pheno[i + 1, j] != 1)){
          //        //Console.WriteLine(" 4th if block - SetScore func....");
          //        score -= 3; } 
          //}
        }// end scoring helper - routine

        /// <summary>
        /// Default constructor probably not helpfull
        /// </summary>
        public Phenotype()
        {
            // default is all null - no need for code yet
        }

        /// <summary>
        /// This is the critical constructor it creates the pheno array for scoring
        /// </summary>
        /// <param name="gg"></param>
        public Phenotype(Genotype gg, int generationCount)
        {
            genotype = gg;
            createPheno();
            setScore();

            gen = generationCount;

        }

        /// <summary>
        ///  create the pheno array
        /// </summary>
        public void createPheno()
        {
            pheno = new int[Params.dimX, Params.dimY];
            for (int x=0; x< Params.dimX;x++)
                for (int y=0; y< Params.dimY;y++) { pheno[x,y] = 0; } // initialise to 0

            for (int i = 0; i < Params.genotypeSize; i++)
            {
                Gene g = genotype.genes[i];
                for (int kx = 0; kx < g.repeatX; kx++)
                    for (int ky = 0; ky < g.repeatY; ky++)
                    {
                        int x = g.x+kx;
                        int y = g.y+ky;
                    if (y< Params.dimY && x< Params.dimX) pheno[x, y] = g.terrain;
                }

            }
        }

        public int getTerrainSafe(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Params.dimX || y >= Params.dimY) return 0;
            return pheno[x, y];
        }

        /// <summary>
        /// returns the score for selection - also stores it in Phenotype
        /// </summary>
        /// <returns></returns>
        public int setScore()
        {
            int local_score = 0;

            int seaCount = 0;
            int landCount = 0;
            int freshCount = 0;

            //store %s of the pic - land, sea and frechH20 - use these against the Global % contraints on the terrain for scoring also below
            double landPercent = 0.0;
            double seaPercent = 0.0;
            double freshPercent = 0.0;

            int every = 1;
            //give intuitive names to what increments (deltas) will be added/subtracted from the score
            int smallDelta = 1;
            int intermedMediumDelta = 2;
            int mediumDelta = 3;
            int largeDelta = 5;
            int intermediatelargeDeta = 6;
            int vLargeDelta = 7;


            for (int x = 0; x < Params.dimX; x = x + every)
            {
                for (int y = 0; y < Params.dimY; y++){  // { pheno[x,y] = 0; } // initialise to 0

                    // TODO : 
                    if (getTerrainSafe(x, y) != 0 && (bool)(getTerrainSafe(x, y-1) != 0) && (bool)(getTerrainSafe(x+1, y) != 0) && 
                        (bool)(getTerrainSafe(x, y+1) != 0) && (bool)(getTerrainSafe(x-1, y) != 0)) {  
   
                        if ( (x == Params.dimX - 1 )|| (y == Params.dimY - 1) || (x == 0) || (y == 0)) {

                                scoringBordersHelperFunc(x, y);
                        }

                        // TODO : put in equivalents to below's logic here !               !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        else
                        {

                            //Console.WriteLine(z);
                            //create a summation for these'conecting' pixels which can be used in the checks ahead...
                            int phenoTerrainSum = pheno[x - 1, y] + pheno[x + 1, y] + pheno[x, y + 1] + pheno[x, y - 1];

                            //ensure the percentage of land in the picture is less than 70% -- also try to encourage land to at least be greater than 35% of th epic's area.
                            if(landPercent > Params.percentLand || landPercent < 0.475) {
                                //Console.WriteLine(" 1st if block - SetScore func  --- chekcin land % -- - score down");
                                score -= vLargeDelta;
                            }
                            
                            //if (landPercent < Params.percentLand && landPercent > 0.475)
                            //{
                                //Console.WriteLine(" 1st if block - SetScore func  --- chekcin land % -- '-' -ive score...");
                            //    score += vLargeDelta;
                            //}
                            
                            //ensure the percentage of land in the picture is less than 70%
                            if (freshPercent > Params.percentFresh) {
                                //Console.WriteLine(" 1st if block - SetScore func  --- chekcin fresh % -- -ive score...");
                                score -= intermediatelargeDeta;
                            }
                            
                            //ensure the percentage of land in the picture is less than 70%
                            //if (freshPercent < Params.percentFresh)
                            //{
                                //Console.WriteLine(" 1st if block - SetScore func  --- chekcin fresh % -- + score...");
                                score += intermediatelargeDeta;
                            //}

                            // Just focus on the simpler  horizontal and vertical connecting
                            //current is on land and one neighbour is water - score up 
                            if (pheno[x, y] == 1)
                            {
                                //Console.WriteLine(" 2nd if block - SetScore func  --- chekcin land one pixed conected to water  % -- + score...");
                            
                                landCount++;
                                if ((landCount + seaCount + freshCount) != 0)
                                {
                                    landPercent = (landCount) / (landCount + seaCount + freshCount);
                                }
                                else
                                {
                                    landPercent = 0.0;
                                }
                            
                                if (pheno[x - 1, y] != 1 | pheno[x + 1, y] != 1 || pheno[x, y + 1] != 1 || pheno[x, y - 1] != 1)
                                {
                                    //Just print for debug puposes
                                    //Console.WriteLine("1st if block :  in land and one neighbour is water - score up part of setScore func. ");
                                    score += smallDelta;
                                }
                            }

                             //land surrounded by water - score down
                             //these sums indicted > 6 or < 2 would correspond to a piece of land mostly (75%) surrounded by sea or fresh water.
                            if (pheno[x, y] == 1 && (phenoTerrainSum > 6 | phenoTerrainSum < 2))
                            {
                                //Console.WriteLine(" 3rd if block - SetScore func  --- chekcin land mostly connected to water  % -- - score...");
                            
                                //Console.WriteLine("2nd if block :  in land and one neighbour is water - score up part of setScore func. ");
                                score -= intermedMediumDelta;
                                local_score -= intermedMediumDelta;
                            }

                            //land connected to mostly other land (say at least 75 % of horizonal & vert. cells) - score up

                            if ((getTerrainSafe(x, y + 2) != 0) && (bool)(getTerrainSafe(x + 1, y + 2) != 0) && (bool)(getTerrainSafe(x + 2, y + 2) != 0) &&
                                (bool)(getTerrainSafe(x - 1, y + 2) != 0) && (bool)(getTerrainSafe(x - 2, y + 2) != 0) && (bool)(getTerrainSafe(x, y - 2) != 0) &&
                                (bool)(getTerrainSafe(x + 1, y - 2) != 0) && (bool)(getTerrainSafe(x + 2, y - 2) != 0) && (bool)(getTerrainSafe(x - 1, y - 2) != 0) &&
                                (bool)(getTerrainSafe(x - 2, y - 2) != 0) && (bool)(getTerrainSafe(x - 2, y) != 0) && (bool)(getTerrainSafe(x - 2, y - 1) != 0) &&
                                (bool)(getTerrainSafe(x - 1, y + 1) != 0) && (bool)(getTerrainSafe(x + 2, y - 1) != 0) && (bool)(getTerrainSafe(x + 2, y + 1) != 0) &&
                                (bool)(getTerrainSafe(x + 2, y) != 0))
                            {

                                // Attempt made here to also look at the next level of 'pixels ' surrounding (x,y) , eg @ (x+2,y), (y-2,x-2) .. etc
                                //I did this to try improve the 256x256 bitmap's evolution...
                                if (pheno[x, y] == 1 && ((pheno[x - 1, y] == 1 && pheno[x + 1, y] == 1 && pheno[x, y + 1] == 1 && pheno[x, y - 1] != 1)
                                                || (pheno[x - 1, y] == 1 && pheno[x + 1, y] == 1 && pheno[x, y - 1] == 1 && pheno[x, y + 1] != 1)
                                                || (pheno[x, y - 1] == 1 && pheno[x + 1, y] == 1 && pheno[x, y + 1] == 1 && pheno[x - 1, y] != 1)
                                                || (pheno[x - 1, y] == 1 && pheno[x, y - 1] == 1 && pheno[x, y + 1] == 1 && pheno[x + 1, y] != 1)
                                                || (pheno[x - 1, y] == 1 && pheno[x, y - 1] == 1 && pheno[x, y + 1] == 1 && pheno[x + 1, y] == 1)
                                                && pheno[x, y + 2] == 1 && pheno[x + 1, y + 2] == 1 && pheno[x + 2, y + 2] == 1 && pheno[x - 1, y + 2] == 1 &&
                                                pheno[x - 2, y + 2] == 1 &&
                                                pheno[x, y - 2] == 1 && pheno[x + 1, y - 2] == 1 && pheno[x + 2, y - 2] == 1 && pheno[x - 1, y - 2] == 1 &&
                                                pheno[x - 2, y - 2] == 1 && pheno[x - 2, y] == 1 && pheno[x - 2, y - 1] == 1 && pheno[x - 1, y + 1] == 1 &&
                                                pheno[x + 2, y - 1] == 1 && pheno[x + 2, y + 1] == 1 && pheno[x + 2, y] == 1))
                                {
                                    //Console.WriteLine(" 4th if block - SetScore func  --- chekcin land connected to more lanb mostly  % -- + score...");

                                    //Console.WriteLine(" 3rd if block - land connected to mostly other land - score up part of setScore func. ");
                                    score += largeDelta;

                                }

                            }

                            //&& (pheno[x , y + 2] == 1 && pheno[x +1, y+2] == 1 && pheno[x+2, y+2] == 1 && pheno[x - 1, y+2] == 1 &&
                            //                pheno[x -2, y + 2] == 1 ||
                            //                
                            //                
                            //                (pheno[x, y - 2] == 1 && pheno[x + 1, y -2] == 1 && pheno[x + 2, y -2] == 1 && pheno[x - 1, y-2] == 1 &&
                            //                pheno[x - 2, y-2] == 1 ||
                            //
                            //
                            //
                            //                pheno[x - 2, y ] == 1 && pheno[x - 2, y -1] == 1 && pheno[x - 1, y +1] == 1 &&
                            //
                            //
                            //                pheno[x + 2, y - 1] == 1 && pheno[x + 2, y + 1] == 1 && pheno[x + 2, y ] == 1)
                            //                pheno[x + 1, y - 1] == 1 && pheno[x - 1, y - 1] == 1 && pheno[x - 1, y + 1] != 1)


                            //land connected to mostly other land- this time all 8 locations 'touching it' (say at least 75 % of horizonal & vert. cells) - score up
                            if (pheno[x, y] == 1 && ((pheno[x - 1, y] == 1 && pheno[x + 1, y] == 1 && pheno[x, y + 1] == 1 && pheno[x, y - 1] != 1)
                                            || (pheno[x - 1, y] == 1 && pheno[x + 1, y] == 1 && pheno[x, y - 1] == 1 && pheno[x - 1, y] == 1 && 
                                            pheno[x + 1, y+1] == 1 && pheno[x+1, y - 1] == 1 && pheno[x - 1, y - 1] == 1 && pheno[x-1, y + 1] != 1)
                                            || (pheno[x, y - 1] == 1 && pheno[x + 1, y] == 1 && pheno[x, y + 1] == 1 && pheno[x + 1, y + 1] == 1 && 
                                            pheno[x + 1, y - 1] == 1 && pheno[x - 1, y - 1] == 1 && pheno[x - 1, y-1] != 1)
                                            || (pheno[x - 1, y] == 1 && pheno[x, y - 1] == 1 && pheno[x, y + 1] == 1 &&  pheno[x + 1, y + 1] == 1 && 
                                            pheno[x + 1, y - 1] == 1 && pheno[x - 1, y - 1] == 1 && pheno[x + 1, y-1] != 1)
                                            || (pheno[x - 1, y] == 1 && pheno[x, y - 1] == 1 && pheno[x, y + 1] == 1 && pheno[x + 1, y] == 1  &&
                                            pheno[x + 1, y + 1] == 1 && pheno[x + 1, y - 1] == 1 && pheno[x - 1, y - 1] == 1 && pheno[x - 1, y + 1] == 1)))
                            {
                                //Console.WriteLine(" 5th if block - SetScore func  --- chekcin land connected to more land * PIXEL VErsion mostly  % -- + score...");

                                //Console.WriteLine(" 3rd if block - land connected to mostly other land - score up part of setScore func. ");
                                score += 9;
                                local_score += 9;
                            }

                            // 	If the cell is water  and more or less surrounded by land,  score it down	
                            //Splitting this into two separate loops so that i can increment the sea and freshwater counts properly.
                            if (pheno[x, y] == 0)
                            {


                                seaCount++;

                                if ((landCount + seaCount + freshCount) != 0) {
                                    seaPercent = (seaCount) / (landCount + seaCount + freshCount);
                                }
                                else
                                {
                                    seaPercent = 0.0;
                                }



                                if ((pheno[x - 1, y] == 1 && pheno[x + 1, y] == 1 && pheno[x, y + 1] == 1 && pheno[x, y - 1] != 1)
                                            || (pheno[x - 1, y] == 1 && pheno[x + 1, y] == 1 && pheno[x, y - 1] == 1 && pheno[x, y + 1] != 1)
                                            || (pheno[x, y - 1] == 1 && pheno[x + 1, y] == 1 && pheno[x, y + 1] == 1 && pheno[x - 1, y] != 1)
                                            || (pheno[x - 1, y] == 1 && pheno[x, y - 1] == 1 && pheno[x, y + 1] == 1 && pheno[x + 1, y] != 1)
                                            || (pheno[x - 1, y] == 1 && pheno[x, y - 1] == 1 && pheno[x, y + 1] == 1 && pheno[x + 1, y] == 1))
                                {
                                    //Console.WriteLine(" 4th if block - SetScore func....");
                                    //Console.WriteLine(" 6th if block - SetScore func  --- chekcin lsea connected to land mostly  % -- + score...");

                                    score -= mediumDelta;
                                }
                            }
                            //same for fresh H2O
                            if (pheno[x, y] == 2)
                            {
                                freshCount++;

                                freshPercent = 1.0 - (seaPercent + landPercent);

                                if ((pheno[x - 1, y] == 1 && pheno[x + 1, y] == 1 && pheno[x, y + 1] == 1 && pheno[x, y - 1] != 1)
                                            || (pheno[x - 1, y] == 1 && pheno[x + 1, y] == 1 && pheno[x, y - 1] == 1 && pheno[x, y + 1] != 1)
                                            || (pheno[x, y - 1] == 1 && pheno[x + 1, y] == 1 && pheno[x, y + 1] == 1 && pheno[x - 1, y] != 1)
                                            || (pheno[x - 1, y] == 1 && pheno[x, y - 1] == 1 && pheno[x, y + 1] == 1 && pheno[x + 1, y] != 1)
                                            || (pheno[x - 1, y] == 1 && pheno[x, y - 1] == 1 && pheno[x, y + 1] == 1 && pheno[x + 1, y] == 1))
                                {

                                    //Console.WriteLine(" 6th if block PART 2 - SetScore func  --- chekcin Fresh  connected to land mostly  % -- + score...");

                                    //Just for debug puposes
                                    //Console.WriteLine(" 4th if block - SetScore func....");
                                    score -= mediumDelta;
                                   
                                }
                            }

                            // !!  CHANGE THIS INTO 2 SPECIFIC LOOPS FOR SEA AND FRESH - TO BE MORE SENSIBLE - DONT MIX THEM !
                            //Look at each cell 	If the cell is water and more or less surrounded by water,  score it up
                            if (pheno[x, y] != 1 && ((pheno[x - 1, y] == 1 && pheno[x + 1, y] == 1 && pheno[x, y + 1] != 1 && pheno[x, y - 1] == 1)
                                            || (pheno[x - 1, y] != 1 && pheno[x + 1, y] != 1 && pheno[x, y - 1] != 1 && pheno[x, y + 1] == 1)
                                            || (pheno[x, y - 1] != 1 && pheno[x + 1, y] != 1 && pheno[x, y + 1] != 1 && pheno[x - 1, y] == 1)
                                            || (pheno[x - 1, y] != 1 && pheno[x, y - 1] != 1 && pheno[x, y + 1] != 1 && pheno[x + 1, y] == 1)))
                            {
                            //Console.WriteLine(" 5th if block - SetScore func....");

                                score += 1;
                                local_score += 1;
                            }

                            //Look at each cell If the cell is Sea and more or less surrounded by freshwater,  score it down
                           if (pheno[x, y] == 2 && ((pheno[x - 1, y] == 0 && pheno[x + 1, y] == 0 && pheno[x, y + 1] == 0 && pheno[x, y - 1] != 0)
                                           || (pheno[x - 1, y] == 0 && pheno[x + 1, y] == 0 && pheno[x, y - 1] == 0 && pheno[x, y + 1] != 0)
                                           || (pheno[x, y - 1] == 0 && pheno[x + 1, y] == 0 && pheno[x, y + 1] == 0 && pheno[x - 1, y] != 0)
                                           || (pheno[x - 1, y] == 0 && pheno[x, y - 1] == 0 && pheno[x, y + 1] == 0 && pheno[x + 1, y] != 0)))
                           {
                               //Console.WriteLine(" 7th if block - SetScore func.. -- Sea mostly surrounded by fresh - score down ");
                           
                               score -= smallDelta;
                             
                           }
                           
                           //Look at each cell If the cell is freshwater and more or less surrounded by Sea,  score it down
                           if (pheno[x, y] == 1 && ((pheno[x - 1, y] == 2 && pheno[x + 1, y] == 2 && pheno[x, y + 1] == 2 && pheno[x, y - 1] != 2)
                                           || (pheno[x - 1, y] == 2 && pheno[x + 1, y] == 2 && pheno[x, y - 1] == 2 && pheno[x, y + 1] != 2)
                                           || (pheno[x, y - 1] == 2 && pheno[x + 1, y] == 2 && pheno[x, y + 1] == 2 && pheno[x - 1, y] != 2)
                                           || (pheno[x - 1, y] == 2 && pheno[x, y - 1] == 2 && pheno[x, y + 1] == 2 && pheno[x + 1, y] != 2)))
                           {
                               //Console.WriteLine(" 7th if block PART 2 - SetScore func.. -- FRESH mostly surrounded by SEA - score down ");
                           
                               score -= smallDelta;
                             
                           }

                        }// end else loop - check for border 'pixels'

                    }// end getTerrain() check on (x,y) co-ords

                }//end inner - y for loop

             }//end inner - x for loop
                 
            //method 1  - cell is on land & more or less surrounded by H2O:

            return score;
        }

        /// <summary>
        /// Display the map in a picturebox
        /// </summary>
        public void show(PictureBox pb)
        {
            System.Drawing.SolidBrush myBrush;
            if (bitm == null)
            {
                bitm = new Bitmap(Params.dimX, Params.dimY);
                myBrush = new System.Drawing.SolidBrush(G.ca[0]);
                Graphics gra = Graphics.FromImage(bitm);

                gra.FillRectangle(myBrush,0,0, Params.dimX, Params.dimY); //this is your code for drawing rectangles
                
                for (int x=0; x< Params.dimX; x++)
                {
                    for (int y = 0; y < Params.dimY; y++)
                    {
                        if (pheno[x,y] > 0)
                        {
                            bitm.SetPixel(x, y, G.ca[pheno[x,y]]);
                        }
                    }
                }
            }
            pb.Image = bitm;
        }
    }
}
