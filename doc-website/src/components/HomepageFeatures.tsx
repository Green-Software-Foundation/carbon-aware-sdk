import useBaseUrl from '@docusaurus/useBaseUrl';
import React from 'react';
import clsx from 'clsx';
import styles from './HomepageFeatures.module.css';

type FeatureItem = {
  title: string;
  image: string;
  description: JSX.Element;
};

const FeatureList: FeatureItem[] = [
  {
    title: 'Use in your CICD workflow',
    image: '/img/undraw_docusaurus_mountain.svg',
    description: (
      <>
        <b>Highly Recommended</b> - This provides your team with the possibility to deploy your worloads to Regions with least emissions.
      </>
    ),
  },
  {
    title: 'Deploy as a REST end point',
    image: '/img/undraw_docusaurus_tree.svg',
    description: (
      <>
        <b>Highly Recommended</b> - Best for when you can change the code, and deploy separately. This also allows you to manage the Carbon Aware logic independently of the system using it.
      </>
    ),
  },
  {
    title: 'Invoke via command line',
    image: '/img/undraw_docusaurus_mountain.svg',
    description: (
      <>
        Best for use with systems you can not change the code in but can invoke command line.
      </>
    ),
  },
  {
    title: 'Write code against the .NET library',
    image: '/img/undraw_docusaurus_react.svg',
    description: (
      <>
        Best for when you are using .NET, and you have the ability to change the code, and do not have the ability to deploy the WebApi.
      </>
    ),
  },
];

function Feature({title, image, description}: FeatureItem) {
  return (
    <div className={clsx('col col--6')}>
      <div className="text--center">
        <img
          className={styles.featureSvg}
          alt={title}
          src={useBaseUrl(image)}
        />
      </div>
      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): JSX.Element {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
